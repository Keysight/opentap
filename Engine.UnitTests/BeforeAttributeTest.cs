using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
namespace OpenTap.Engine.UnitTests
{
    
    
    
    [TestFixture]
    public class BeforeAttributeTest
    {
        public class StringConvertProviderA : IStringConvertProvider
        {
            public static List<Type> UseOrder = new List<Type>();
            public string GetString(object value, CultureInfo culture)
            {
                if (UseOrder.Contains(GetType()) == false)
                {
                    UseOrder.Add(GetType());
                }
                return null;
            }
        
            public object FromString(string stringData, ITypeData type, object contextObject, CultureInfo culture)
            {
                return null;
            }
        }

        [Before(typeof(StringConvertProviderC))]
        [Before(typeof(StringConvertProviderD))]
        public class StringConvertProviderB : StringConvertProviderA
        {
            
        }
        
        [Before(typeof(StringConvertProviderA))]
        public class StringConvertProviderC : StringConvertProviderA
        {
            
        }
        [Before(typeof(StringConvertProviderC))]
        public class StringConvertProviderD : StringConvertProviderA
        {
            
        }
        
        public void TestBeforeAttribute()
        {
            StringConvertProvider.GetString(new StringConvertProviderD());
            int ia, ib, ic, id;
            ia = StringConvertProviderA.UseOrder.IndexOf(typeof(StringConvertProviderA));
            ib = StringConvertProviderA.UseOrder.IndexOf(typeof(StringConvertProviderB));
            ic = StringConvertProviderA.UseOrder.IndexOf(typeof(StringConvertProviderC));
            id = StringConvertProviderA.UseOrder.IndexOf(typeof(StringConvertProviderD));
            Assert.IsTrue(ib < ic);
            Assert.IsTrue(ib < id);
            Assert.IsTrue(ic < id);
            Assert.IsTrue(ia < ic);

            
        }
        
    }
}
