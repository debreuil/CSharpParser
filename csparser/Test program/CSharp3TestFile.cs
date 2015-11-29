using System.Text;
using System.Threading;

namespace DDW
{
    /// <summary>
    /// Please do not delete this file.
    /// I will use it to test C# 3.0 compliance
    /// </summary>
    class CSharp3TestFile
    {
        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        /// <summary>
        /// Auto-properties must have both get and set.
        /// Modifiers are allowed
        /// </summary>
        public int a { 
            get; 
            protected set; 
        }

        /// <summary>
        /// Var
        /// </summary>
        public void A()
        {
            var v = new StringBuilder();
        }

        /// <summary>
        /// Object initializer
        /// </summary>
        public void B()
        {
            var p = new Person
                           {
                               Age = 36,
                               Name = "John Doe"
                           };
        }

        /// <summary>
        /// Lambda
        /// </summary>
        public void C()
        {
            ThreadStart ts = (() => B());
        }

        /// <summary>
        /// Array initializer
        /// </summary>
        public void D()
        {
            var persons = new[]
                              {
                                  new Person
                                      {
                                          Age = 36,
                                          Name = "John Doe"
                                      },
                                  new Person
                                      {
                                          Age = -3,
                                          Name = "Alien from Mars"
                                      }
                              };
        }
    }
}
