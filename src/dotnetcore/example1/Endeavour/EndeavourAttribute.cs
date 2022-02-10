using System;
using System.Diagnostics;
using System.Text;


namespace Endeavour
{

    public readonly struct EndeavourToGenerate
    {
        public readonly string Name;
        public readonly Type Type;

        public readonly string ClassName;

        public EndeavourToGenerate(string className, string name, Type type)
        {
            ClassName = className;
            Name = name;
            Type = type;
        }
    }

  [System.AttributeUsage(AttributeTargets.Class)]
    public class EndeavourAttribute<T> : Attribute
    {
 public EndeavourAttribute(string name)
  {
            Name = name;
        }
      
          public string Name { get; set; }
    }

  


   // [EndeavourAttribute<TesterDto>("Customer")]
    public partial class Tester
    {
    }



    public static class SimHost
    {
        public static void Test()
        {
            var t = new Tester();
          //  t.Testing123();
        }
    }

    public class TesterDto
    {

    }

}