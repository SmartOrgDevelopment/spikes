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
                for (int i = 0; i < 30; i++)
                {
                    var id = i;
                    var time = i * 100;
                    var square = double.MaxValue - i;

                    Console.WriteLine("Starting {0}", id);

                    q.Start(delegate
                    {
                        Console.WriteLine("Computing {0}", id);
                        Thread.Sleep(time);
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
                        throw result.Error;

                    Console.WriteLine(result.Value);

                    count++;
                }

                Console.WriteLine("\n###\nCount = {0}", count);
            }

            Console.ReadLine();

            Debugger.Break();
        }

        public static void Test(IEnumerable<int> ids)
        {
            using (AsyncWorkQueue<Company> q = new AsyncWorkQueue<Company>(5))
            {
                foreach (int id in ids)
                {
                    int myId = id;

                    q.Start(delegate { return GetCompany(myId); });
                }


            }
        }

        private static Company GetCompany(int id)
        {
            return new Company();
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
