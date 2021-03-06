﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Monocle.Utils;

namespace Performance
{
    class MessagePerformance
    {
        const long TIME = 1000;
        const long TRIES = 100000;

        public void Run(long time, long tries)
        {
            MessageSender.CacheAssemblieMessages(typeof(MessA).Assembly);
            MessA messa = new MessA();
            ScriptMethod del = new ScriptMethod("Method2");
            del.TrackInstance(messa);


            long dC = 0, mC = 0, sM = 0;
            for (int i = 0; i < tries; i++)
            {
                dC += Take_Time(time, (num, m) => m.Method2());
                mC += Take_Time(time, (num, m) => MessageSender.SendMessage(m, "Method2", null, MessageOptions.DontRequireReceiver));
                sM += Take_Time(time, (num, m) => del.Method.Invoke());
            }

            Console.WriteLine("Direct_Call mean: " + ((decimal)dC / (decimal)tries));
            Console.WriteLine("Message_Call mean: " + ((decimal)mC / (decimal)tries));
            Console.WriteLine("ScripMethod_Call mean: " + ((decimal)sM / (decimal)tries));
            Console.WriteLine("The ratio of calls is: " + ((decimal)mC / (decimal)dC));
        }


        public long Take_Time(long time, Action<int, Mess> testFuncion)
        {
            var watch = new Stopwatch();
            Mess mess = new MessA();
            watch.Start();
            for (long i = 0; i < time; i++)
            {
                testFuncion(5, mess);
            }

            watch.Stop();

            return watch.ElapsedTicks;
        }
    }

    interface Mess
    {
        void Method1(string value);
        void Method2();
    }

    class MessA : IReceiver, Mess
    {
        [Message]
        public void Method1(string value) { }
        
        [Message]
        public void Method2() { }

        public void SendMessage(string messageName, object param, MessageOptions opt = MessageOptions.DontRequireReceiver)
        {
            MessageSender.SendMessage(this, messageName, param, opt);
        }
    }
}
