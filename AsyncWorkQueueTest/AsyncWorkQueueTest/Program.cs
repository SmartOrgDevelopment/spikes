using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SpreadsheetGear;

namespace AsyncWorkQueueTest
{
    class Program
    {
        private static double ONE_MILLION = 1000000.0;
        public static int generate(int low, int high)
        {
            Random random = new Random();
            return random.Next(low, high);
        }
        public static void Main(string[] args)
        {
            PointEstimate estimate = new PointEstimate();
            Stopwatch sw = Stopwatch.StartNew();
            using (var q = new AsyncWorkQueue<double>(3))
             {
                for (int i=0;i<3;i++) {
                    // 3 processors
                    var id = i;
                    q.Start(delegate
                    {
                        SpreadsheetGear.IWorkbookSet workbookSet = SpreadsheetGear.Factory.GetWorkbookSet();
                        IWorkbook workbook = workbookSet.Workbooks.Open(@"C:\data\dev\Async\AsyncWorkQueueTest\AsyncWorkQueueTest\excel\helloWorld_template.xls");
                        for (long j = 0; j < ONE_MILLION/3; j++)
                        {
                            try
                            {
                                double annRevenue = generate(30, 70);
                                double annCosts = generate(5, 20);
                                double numYears = generate(2, 6);
                                var sheet = workbook.Worksheets["Hello"];
                                sheet.Cells["AnnRevenue"].Value = Convert.ToDouble(annRevenue);
                                sheet.Cells["AnnCosts"].Value = Convert.ToDouble(annCosts);
                                sheet.Cells["numYears"].Value = Convert.ToDouble(numYears);
                                var npvRange = sheet.Cells["npv"];
                                double npv = (double)npvRange.Value;
                                estimate.nextValueIs(npv);
                                /*if (j % 50000 == 0)
                                {
                                    Console.WriteLine("Process "+id+ ", Runs: "+ j);
                                }*/
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Process " + id + ", run " + j + ", exception thrown!");
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                        return 0;
                    });
                    Console.WriteLine("Started delegate " + i);
                }
                
                 Console.WriteLine("*** Done queueing.");
                

                 foreach (var result in q.GetResults())
                 {
                     if (result.IsError)
                         //handle / log errors here
                         throw result.Error;
                 }
                 SummaryStatistics statistics = estimate.computeConfidenceIntervalForPercent(95);
                 sw.Stop();
                 Console.WriteLine("Mean: " + statistics.pointEstimate + ", [" + statistics.cLower + ", " + statistics.cUpper + "]");
                 Console.WriteLine("Elapsed time = " + sw.Elapsed);
             }

            
            
            //Test(new List<int>() { 5, 6, 5, 4, 3, 2, 1 });
             
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
