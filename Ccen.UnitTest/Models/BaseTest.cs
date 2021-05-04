using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.UnitTest.Models
{
    public abstract class BaseTest
    {
        protected CcenTestContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = new CcenTestContext();
            _context.Setup();
        }
    }
}
