/** 
 * Copyright 2016 Telerik AD                                                                                                                                                                            
 *                                                                                                                                                                                                      
 * Licensed under the Apache License, Version 2.0 (the "License");                                                                                                                                      
 * you may not use this file except in compliance with the License.                                                                                                                                     
 * You may obtain a copy of the License at                                                                                                                                                              
 *                                                                                                                                                                                                      
 *     http://www.apache.org/licenses/LICENSE-2.0                                                                                                                                                       
 *                                                                                                                                                                                                      
 * Unless required by applicable law or agreed to in writing, software                                                                                                                                  
 * distributed under the License is distributed on an "AS IS" BASIS,                                                                                                                                    
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.                                                                                                                             
 * See the License for the specific language governing permissions and                                                                                                                                  
 * limitations under the License.                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       

*/

(function(f){
    if (typeof define === 'function' && define.amd) {
        define(["kendo.core"], f);
    } else {
        f();
    }
}(function(){
(function( window, undefined ) {
    kendo.cultures["zh-TW"] = {
        name: "zh-TW",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": ",",
            ".": ".",
            groupSize: [3],
            percent: {
                pattern: ["-n%","n%"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                name: "New Taiwan Dollar",
                abbr: "TWD",
                pattern: ["-$n","$n"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "NT$"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["?????????","?????????","?????????","?????????","?????????","?????????","?????????"],
                    namesAbbr: ["??????","??????","??????","??????","??????","??????","??????"],
                    namesShort: ["???","???","???","???","???","???","???"]
                },
                months: {
                    names: ["??????","??????","??????","??????","??????","??????","??????","??????","??????","??????","?????????","?????????"],
                    namesAbbr: ["??????","??????","??????","??????","??????","??????","??????","??????","??????","??????","?????????","?????????"]
                },
                AM: ["??????","??????","??????"],
                PM: ["??????","??????","??????"],
                patterns: {
                    d: "yyyy/M/d",
                    D: "yyyy'???'M'???'d'???'",
                    F: "yyyy'???'M'???'d'???' tt hh:mm:ss",
                    g: "yyyy/M/d tt hh:mm",
                    G: "yyyy/M/d tt hh:mm:ss",
                    m: "M'???'d'???'",
                    M: "M'???'d'???'",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "tt hh:mm",
                    T: "tt hh:mm:ss",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "yyyy'???'M'???'",
                    Y: "yyyy'???'M'???'"
                },
                "/": "/",
                ":": ":",
                firstDay: 0
            }
        }
    }
})(this);
}));