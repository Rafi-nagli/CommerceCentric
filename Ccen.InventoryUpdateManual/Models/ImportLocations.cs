using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.DAL;
using Amazon.InventoryUpdateManual.ExcelModel;
using CsvHelper;
using CsvHelper.Configuration;
using log4net;

namespace Amazon.InventoryUpdateManual
{
    public class ImportLocations
    {
        private ILog _logger;

        public ImportLocations(ILog logger)
        {
            _logger = logger;
        }

        public class ImportLocation
        {
            public string Style { get; set; }
            public string Isle { get; set; }
            public string Section { get; set; }
            public string Shelf { get; set; }
            public bool IsDefault { get; set; }
        }

        //RULES:
        //1.	Есть некоторые стили которые начинаются на 21, и у которых между Excel и нашей системой отличаются только 2 последние буквы. 
        //Это точно надо игнорировать и match them (последние 2 буквы у стилей начинающихся на 21 не реалевантны)

        //2.   - получается что мы сейчас маппим на
        //21IUS186SLFPDZ два Style из excel 21US186ZSFPZA и 21US186SLFPDZ, 
        //их помечать как Exception уже не нужно?
        //[RN] держи как exception, but map them
        //- сейчас location помеченные как Exception не импортируются, это правильно?
        //[RN] если один из стайлов заканчивается на DZ сделай его дефолтным , а другой нет. Замап их. Остальные пока остав not mapped.
        //Пришли файлик плз.
 
        //3. Please match following styles:
        //excel				Our system
        //21US186ZSFPZA	Can't find style	1 / 4 / 2 Y	listed US186	21IUS186SLFPDZ
        //21SH006SLFPZA	Can't find style	2 / 2 / 1,3 Y	listed	21SH006SLBPZA
        //21US186SLFPDZ	Can't find style	2 / 3 / 2 Y	listed US186	21IUS186SLFPDZ
        //21US186ZSFPZA	Can't find style	2 / 5 / 3 N	listed US186	21IUS186SLFPDZ
        //K166406DM	Can't find style	6 / 5 / 1-3 Y	listed	K166406DM
        //K1666611SC	Can't find style	7 / 6 / 1 Y	listed	K1666611SC
        //FZ057GRDTG	Can't find style	10 / 3 / 1 Y	listed Robe 	21FZ057G
        //21FZ039GGSZA	Can't find style	10 / 5-6 / 1 Y	listed	21FZ039GDSZA
        //21LM016TGSZA	Can't find style	10 / 7 / 1,3 Y	listed	21LM016GGSZA
        //K157534PN	Can't find style	13 / 5-6 / 1 Y	listed	K157534PN

        public void Import(string filePath, string outputPath, bool processLocationIfStyleHaventAny, int[] processingOnlyThisIsles)
        {
            _logger.Info("Start importing...");

            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                TrimFields = true,
            });


            var locationResults = new List<LocationResult>();

            //One excel style mapped to two or more styles
            var excelStyleToOurStyle = new Dictionary<string, List<string>>();
            
            var excelLocations = new List<ImportLocation>();

            using (var db = new UnitOfWork(null))
            {
                var allDbStyles = db.Styles.GetAll().Where(s => !s.Deleted).ToList();

                //STEP 1 (file processing, creating mapping)
                //- Filling excel location list
                //- Filling mapping of style Id in excel file to our system style Id
                while (reader.Read())
                {
                    string style = reader.CurrentRecord[0];
                
                    if (String.IsNullOrEmpty(style))
                        continue;
                    style = StringHelper.RemoveWhitespace(style);

                    string isle = reader.CurrentRecord[1];
                    string section = reader.CurrentRecord[2];
                    string shelf = reader.CurrentRecord[3];
                    bool isDefault = reader.CurrentRecord[4] == "Y";

                    //Skipping all other isles
                    if (processingOnlyThisIsles != null && !processingOnlyThisIsles.Any(i => i.ToString() == isle))
                        continue;

                    excelLocations.Add(new ImportLocation()
                    {
                        Style = style,
                        Isle = isle,
                        Section = section,
                        Shelf = shelf,
                        IsDefault = isDefault
                    });

                    var dbStyles = FindByMatchRules(allDbStyles, style);
                    if (dbStyles.Count == 0)
                    {
                        dbStyles = allDbStyles.Where(s => (s.StyleID.Contains(style) || style.Contains(s.StyleID)) && !s.Deleted).ToList();
                    }
                    if (style.StartsWith("21") && dbStyles.Count == 0)
                    {
                        dbStyles = allDbStyles.Where(s => s.StyleID.Contains(style.Substring(0, style.Length - 2))).ToList();
                        if (dbStyles.Count > 0)
                            _logger.Info("Trying match without last characters, count=" + dbStyles.Count);
                    } 

                    //Fill excel style to our style
                    if (!excelStyleToOurStyle.Keys.Contains(style))
                    {
                        excelStyleToOurStyle.Add(style, dbStyles.Select(s => s.StyleID).Distinct().ToList());
                    }
                    else
                    {
                        var existStyle = excelStyleToOurStyle[style];
                        var styleIds = dbStyles.Select(s => s.StyleID).ToList();
                        foreach (var id in styleIds)
                            if (!existStyle.Contains(id))
                                existStyle.Add(id);
                    }
                }


                //STEP 2 (iterate locations)
                foreach (var location in excelLocations)
                {
                    var ourStyles = excelStyleToOurStyle[location.Style];

                    if (ourStyles.Count == 1) //if one style in our system (to one style in excel / to multiple style in excel)
                    {
                        var ourStyleId = ourStyles[0];
                        var multipleExcelStylesToOurStyles = excelStyleToOurStyle.Where(e => e.Value.Any(v => v == ourStyleId)).Select(e => e.Key).ToList();
                        
                        //Get Ignore Rule if it is not None we can continue if has multiple Excel style to our
                        var ignore = IgnoreDuplicates(multipleExcelStylesToOurStyles, location.Style);
                        
                        if (multipleExcelStylesToOurStyles.Count == 1 || ignore != IgnoreRule.None)
                        {
                            //TASK: если один из стайлов заканчивается на DZ сделай его дефолтным , а другой нет
                            if (multipleExcelStylesToOurStyles.Count > 1 && ignore == IgnoreRule.EndWithDZ)
                            {
                                if (!location.Style.EndsWith("DZ"))
                                    location.IsDefault = false;
                            }
                            if (multipleExcelStylesToOurStyles.Count > 1 && ignore != IgnoreRule.None)
                            {
                                locationResults.Add(new LocationResult()
                                {
                                    StyleInOurSystem = ourStyleId,
                                    SuitableStylesInExcel = String.Join(", ", multipleExcelStylesToOurStyles.ToList()),
                                    ExcelStyle = location.Style,
                                    Comments = "[Mapped] Exception, we should have one to one",
                                    Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                                });
                                _logger.Debug("Exception, we should have one to one: excel styles="
                                              + String.Join(", ", multipleExcelStylesToOurStyles.ToList())
                                              + ", our style=" + ourStyleId);
                            }

                            var results = ProcessLocationWithCheckingAnyExist(db, location, ourStyles[0], processLocationIfStyleHaventAny);
                            locationResults.AddRange(results);
                        }
                        else
                        {
                            if (multipleExcelStylesToOurStyles.Count > 1)
                            {
                                //Contain equal style, skip similar style names 
                                if (ourStyles[0] == location.Style)
                                {
                                    var results = ProcessLocationWithCheckingAnyExist(db, location, ourStyles[0], processLocationIfStyleHaventAny);
                                    locationResults.AddRange(results);
                                }
                                else
                                {
                                    locationResults.Add(new LocationResult()
                                    {
                                        StyleInOurSystem = ourStyleId,
                                        SuitableStylesInExcel = String.Join(", ", multipleExcelStylesToOurStyles.ToList()),
                                        ExcelStyle = location.Style,
                                        Comments = "Exception, we should nave one to one",
                                        Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                                    });
                                    _logger.Debug("Exception, we should nave one to one: excel styles="
                                                  + String.Join(", ", multipleExcelStylesToOurStyles.ToList())
                                                  + ", our style=" + ourStyleId);
                                }
                            }
                            else if (multipleExcelStylesToOurStyles.Count == 0)
                            {
                                locationResults.Add(new LocationResult()
                                {
                                    StyleInOurSystem = "",
                                    SuitableStylesInExcel = location.Style,
                                    ExcelStyle = location.Style,
                                    Comments = "Can't find style",
                                    Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                                });
                                _logger.Debug("Can't find style: " + location.Style);
                            }
                        }
                    }
                    else
                    {
                        if (ourStyles.Count > 1)
                        {
                            //Contain equal style, skip similar style names 
                            if (ourStyles.Any(s => s == location.Style))
                            {
                                var results = ProcessLocationWithCheckingAnyExist(db, location, ourStyles[0], processLocationIfStyleHaventAny);
                                locationResults.AddRange(results);
                            }
                            else
                            {
                                locationResults.Add(new LocationResult()
                                {
                                    StyleInOurSystem = String.Join(", ", ourStyles.Select(s => s).ToList()),
                                    SuitableStylesInExcel = location.Style,
                                    ExcelStyle = location.Style,
                                    Comments = "Exception, we should have one to one",
                                    Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                                });
                                _logger.Debug("Exception, we should have one to one: excel style="
                                              + location.Style
                                              + ", our styles=" + String.Join(", ", ourStyles.Select(s => s).ToList()));
                            }
                        }
                        else
                        {
                            locationResults.Add(new LocationResult()
                            {
                                StyleInOurSystem = "",
                                SuitableStylesInExcel = location.Style,
                                ExcelStyle = location.Style,
                                Comments = "Can't find style",
                                Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                            });
                            _logger.Debug("Can't find style: " + location.Style);
                        }
                    }
                }
            }

            _logger.Info("Write to file, path=" + outputPath);
            WriteToFile(outputPath, locationResults);

            _logger.Info("End import");
        }

        public enum IgnoreRule
        {
            EndWithDZ,
            IgnoreList,
            None
        }

        private IgnoreRule IgnoreDuplicates(IList<string> mappedExcelStyles, string style)
        {
            if ((mappedExcelStyles.Any(s => s.EndsWith("DZ")) && style.EndsWith("ZA")) ||
                 (mappedExcelStyles.Any(s => s.EndsWith("ZA")) && style.EndsWith("DZ")))
                return IgnoreRule.EndWithDZ;

            var styles = new List<string>() {"21IUS186SLFPDZ", "21LM016TGSZA"};
            if (styles.Contains(style))
                return IgnoreRule.IgnoreList;
            return IgnoreRule.None;
        }

        private IList<Style> FindByMatchRules(IList<Style> allDbStyles, string style)
        {
            var styleMap = new Dictionary<string, string>()
            {
                { "21US186ZSFPZA", "21IUS186SLFPDZ" },
                { "21SH006SLFPZA", "21SH006SLBPZA" },
                { "21US186SLFPDZ", "21IUS186SLFPDZ"},
                { "K166406DM", "K166406DM" },
                { "K1666611SC", "K1666611SC" },
                { "FZ057GRDTG", "21FZ057G" },
                { "21FZ039GGSZA", "21FZ039GDSZA" },
                { "21LM016TGSZA", "21LM016GGSZA" },
                { "K157534PN", "K157534PN" }
            };

            if (styleMap.ContainsKey(style))
            {
                var dbStyle = allDbStyles.FirstOrDefault(s => s.StyleID == styleMap[style]);
                if (dbStyle != null)
                    return new List<Style>() { dbStyle };
            }

            return new List<Style>();
        }

        public void WriteToFile(string path, IList<LocationResult> results)
        {
            //using (var stream = ExcelHelper.Export(results))
            //{
            //    stream.Seek(0, SeekOrigin.Begin);
            //    using (var fileStream = File.Create(path))
            //    {
            //        stream.CopyTo(fileStream);
            //    }
            //}
        }

        private bool HasStyleLocation(IUnitOfWork db, string ourStyleId)
        {
            var dbStyle = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == ourStyleId && !s.Deleted);
            var existLocations = db.StyleLocations.GetByStyleId(dbStyle.Id);
            return existLocations.Any();
        }

        private IList<LocationResult> ProcessLocationWithCheckingAnyExist(IUnitOfWork db, ImportLocation location, string ourStyleId, bool processIfHaventAny)
        {
            var results = new List<LocationResult>();
            if (processIfHaventAny)
            {
                if (!HasStyleLocation(db, ourStyleId))
                {
                    ProcessLocation(db, location, ourStyleId);
                    results.Add(new LocationResult()
                    {
                        StyleInOurSystem = ourStyleId,
                        SuitableStylesInExcel = location.Style,
                        ExcelStyle = location.Style,
                        Comments = "[Mapped] Success",
                        Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                    });
                }
                else
                {
                    results.Add(new LocationResult()
                    {
                        StyleInOurSystem = ourStyleId,
                        SuitableStylesInExcel = location.Style,
                        ExcelStyle = location.Style,
                        Comments = "[Not Mapped] Skipped, style already have locations",
                        Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                    });
                }
            }
            else
            {
                ProcessLocation(db, location, ourStyleId);
                results.Add(new LocationResult()
                {
                    StyleInOurSystem = ourStyleId,
                    SuitableStylesInExcel = location.Style,
                    ExcelStyle = location.Style,
                    Comments = "[Mapped] Success",
                    Locations = location.Isle + " / " + location.Section + " / " + location.Shelf + " " + (location.IsDefault ? "Y" : "N")
                });
            }
            return results;
        }


        private void ProcessLocation(IUnitOfWork db, ImportLocation location, string ourStyleId)
        {
            int firstIsle = GetInts(location.Isle).First();
            int firstSection = GetInts(location.Section).First();
            int firstShelf = GetInts(location.Shelf).First();

            var dbStyle = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == ourStyleId && !s.Deleted);
            if (dbStyle == null)
            {
                throw new ArgumentOutOfRangeException("can't found style: " + ourStyleId);
            }

            var dbExistLocations = db.StyleLocations.GetByStyleId(dbStyle.Id);
            var dbExistLocation = dbExistLocations.FirstOrDefault(l => l.Isle == location.Isle && l.Section == location.Section && l.Shelf == location.Shelf);

            if (dbExistLocation == null)
            {
                var existIsDefault = dbExistLocations.Any(s => s.IsDefault);
                db.StyleLocations.Add(new StyleLocation()
                {
                    StyleId = dbStyle.Id,

                    Isle = location.Isle,
                    Section = location.Section,
                    Shelf = location.Shelf,

                    SortIsle = firstIsle,
                    SortSection = firstSection,
                    SortShelf = firstShelf,

                    IsDefault = location.IsDefault && !existIsDefault,

                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                });
                db.Commit();
                Console.WriteLine("Added: styleId=" + dbStyle.StyleID + ", isle=" + location.Isle + ", section=" + location.Section + ", shelf=" + location.Shelf);
            }
            else
            {
                if (dbExistLocation.IsDefault != location.IsDefault)
                {
                    dbExistLocation.IsDefault = location.IsDefault;
                    db.Commit();
                }
            }
            if (dbExistLocations.Count(s => s.IsDefault) > 1)
            {
                var dbExistDefaultLocations = dbExistLocations.Where(l => l.IsDefault).ToList();
                for (int i = 1; i < dbExistDefaultLocations.Count; i++)
                {
                    dbExistDefaultLocations[i].IsDefault = false;
                }
                db.Commit();
            }
        }

        private IList<int> GetInts(string values)
        {
            var results = new List<int>();
            if (values.Contains("-"))
            {
                var parts = values.Split('-');
                int start = Int32.Parse(parts[0]);
                int end = Int32.Parse(parts[1]);
                for (int i = start; i <= end; i++)
                    results.Add(i);
                return results;
            }
            if (values.Contains(","))
            {
                var parts = values.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    results.Add(Int32.Parse(parts[i]));
                }
                return results;
            }
            return new List<int> { Int32.Parse(values)};
        }
    }
}
