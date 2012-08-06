using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace AsyncWorkQueueTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var q = new AsyncWorkQueue<double>(5))
             {
                 for (int i = 0; i < 12; i++)
                 {
                     var id = i;
                     var time = i * 100;
                     var square = double.MaxValue - i;

                     Console.WriteLine("Starting {0}", id);

                     q.Start(delegate
                     {
                         Console.WriteLine("COMPUTING ...  {0}", id);
                         Thread.Sleep(time);
                         //throw new Exception("blah");
                         var result = Math.Sqrt(square);
                         Console.WriteLine("Done {0}", id);
                         return result;
                     });
                 }

                 Console.WriteLine("*** Done queueing.");

                 int count = 0;
                 foreach (var result in q.GetResults())
                 {
                     if (result.IsError)
                         //handle / log errors here
                         throw result.Error;

                     Console.WriteLine(result.Value);

                     count++;
                 }

                 Console.WriteLine("\n###\nCount = {0}", count);
             }

            
            
            Test(new List<int>() { 5, 6, 5, 4, 3, 2, 1 });
             
            Console.ReadLine();

        }

        public static void Test(IEnumerable<int> ids)
        {
            using (AsyncWorkQueue<Company> q = new AsyncWorkQueue<Company>(3))
            {
                foreach (int id in ids)
                {
                    int myId = id;

                    q.Start(delegate {

                        Console.WriteLine("my id was ... " + myId);
                        Thread.Sleep( 111);
                        Console.WriteLine(" long runnint task ... " + myId);
                        return GetCompany(myId);
                    });
                }

               
                foreach (var result in q.GetResults())
                {
                    if (result.IsError)
                        //handle / log errors here
                        throw result.Error;

                    Company tmpCompany = (Company)result.Value;

                    Console.WriteLine(tmpCompany.Id);

                    
                }



            }
        }

        private static Company GetCompany(int id)
        {
            Company c = new Company();
            c.Id = id;
            return c;
        }

        private class Company
        {
            public int Id;
            public string Name;
            public Employee[] Employees;
        }

        private class Employee{}
    }
}
