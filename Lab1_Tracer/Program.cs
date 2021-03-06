﻿using System;
using System.Threading;
using TraceLib;

namespace Lab1_Tracer
{
    static public class StaticTracer
    {
        public static Tracer t = new Tracer();
    }
    class TestClass1
    {
        public void PrintFunc(string text, int count)
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(5);
            for (int i = 0; i < count; i++)
                Console.WriteLine(text);
            StaticTracer.t.StopTrace();
        }

        public void NestedFunc(int callTimes)
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(5);
            Console.WriteLine(string.Format("NestedFunc was called, {0} calls left", callTimes));
            if (callTimes != 0)
            {
                NestedFunc(callTimes - 1);
            }
                
            Console.WriteLine(string.Format("NestedFunc that had {0} calls left is returning", callTimes));
            StaticTracer.t.StopTrace();
        }

        public void CascadeFunc(int callTimes)
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(5);
            Console.WriteLine(string.Format("CascadeFunc was called, {0} calls left", callTimes));
            if (callTimes != 0)
            {
                CascadeFunc(callTimes - 1);
                CascadeFunc(callTimes - 1);
            }            
            StaticTracer.t.StopTrace();
        }
    }

    class TestClass2
    {
        public void MultiClassFunc()
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(5);
            TestClass1 t1 = new TestClass1();
            for (int i = 0; i < 3; i++)
            {
                t1.PrintFunc(string.Format("MultiClassFunc called print func with arg {0}",i), i);
            }
            t1.NestedFunc(2);
            t1.CascadeFunc(2);
            StaticTracer.t.StopTrace();
        }
    }

    class TestClass3
    {
        public void m1()
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(50);
            StaticTracer.t.StopTrace();

        }
        public void m2()
        {
            StaticTracer.t.StartTrace();
            Thread.Sleep(50);
            StaticTracer.t.StopTrace();

        }
    }


    class Program
    {
        
        static void Main(string[] args)
        {
            var t1 = new Thread(() => {
                StaticTracer.t.StartTrace();
                TestClass2 t2 = new TestClass2();
                t2.MultiClassFunc();
                StaticTracer.t.StopTrace();
            });
            t1.Start();
            var t2 = new Thread(() =>
            {
                StaticTracer.t.StartTrace();
                TestClass3 tmp = new TestClass3();
                tmp.m1();
                tmp.m2();
                StaticTracer.t.StopTrace();
            });
            t2.Start();
            var t3 = new Thread(() =>
            {
                TestClass1 tmp = new TestClass1();
                tmp.PrintFunc("text", 1);
                tmp.PrintFunc("text", 1);
                tmp.PrintFunc("text", 1);
                tmp.PrintFunc("text", 1);
            });
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();
            Console.WriteLine("1 for JSON, 2 for XML");
            int k = 0;
            ISerializer serializer = null;
            while (k != '1' && k != '2')
            {
                k = Console.Read();
                if (k == '1')
                {                
                    serializer = new JSONSerializer();
                }
                else if (k == '2')
                {
                    serializer = new XMLSerializer();               
                }
            }
            byte[] printData = serializer.Serialize(StaticTracer.t.GetTraceResult());
            Console.WriteLine("1 for Console, 2 for file");
            k = Console.Read();
            IOutput o = null;
            while (k != '1' && k != '2')
            {
                k = Console.Read();
                if (k == '1')
                {
                    o = new ConsoleOut();
                }
                else if (k == '2')
                {
                    o = new FileOut("tmp.txt");
                }
            }           
            o.PrintData(printData);
            Console.Read();
        }
    }
}
