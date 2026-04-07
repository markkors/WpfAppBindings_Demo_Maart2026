using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBindings.models
{
    public class Person
    {
        int _id;
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }

        string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        int _age;
        public int age
        {
            get { return _age; }
            set { _age = value; }
        }

        // constructor
        public Person() { }
        public Person(int id, string name, int age)
        {
            _id = id;
            _name = name;
            _age = age;
        }
    }
}
