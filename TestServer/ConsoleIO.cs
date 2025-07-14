// ConsoleIO.cs


using Microsoft.Extensions.Options;

namespace TestServer
{
    internal static class ConsoleIO
    {
        private static string incorrectInput = "Incorrect input. Try again: ";
        //"Некорректные данные. Попробуйте ещё раз: ";
        private static string outOfRange = "Input is out of range. Try again: ";
        //"Число выходит за допустимые пределы. Попробуйте ещё раз: ";

        internal static int GetInt(string msg)
        {
            Console.Write(msg);

            int output;
            while (!int.TryParse(Console.ReadLine(), out output))
            {
                Console.Write(incorrectInput);
            }
            
            return output;
        }


        internal static int GetInt(string msg, int min, int max = int.MaxValue)
        {
            int output;
            while (true)
            {
                output = GetInt(msg);
                if (output <= max && output >= min) break;
                Console.Write(outOfRange);
            }

            return output;
        }


        internal static double GetDouble(string msg) 
        {
            Console.Write(msg);
            double output;
            while(!double.TryParse(Console.ReadLine(), out output)) 
            {
                Console.Write(incorrectInput);
            }

            return output;
        }


        internal static double GetDouble(string msg, double min, double max = double.MaxValue)
        {
            double output;
            while (true)
            {
                output = GetDouble(msg);
                if (output <= max && output >= min) break;
                Console.Write(outOfRange);
            }

            return output;
        }


        internal static bool GetBool(string msg)
        {
            Console.Write(msg);
            bool output;
            while (!bool.TryParse(Console.ReadLine(), out output))
            {
                Console.WriteLine(incorrectInput);
            }
            return output;
        }


        internal static T GetEnum<T>(string msg, bool mustBeDefined = false) where T : struct, Enum
        {
            Console.WriteLine(msg);

            int i = 0;
            foreach(var name in Enum.GetNames<T>())
            {
                Console.WriteLine("  " + (Math.Pow(2, i)) + " - " + name);
                i++;
            }

            T output;
            while (!Enum.TryParse(Console.ReadLine(), out output) ||
                (mustBeDefined && !Enum.IsDefined(typeof(T), output))) 
                Console.WriteLine(incorrectInput);
            
            return output;
        }


        internal static string GetString(string msg)
        {
            Console.Write(msg);
            string? output;
            while (true)
            {
                output = Console.ReadLine();
                if (output != null) break;
                Console.WriteLine(incorrectInput);
            }
            return output;
        }


        internal static void ShowOptions<T>(List<T> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine("  " + (i + 1) + " - " + options[i]);
            }
        }

        internal static void ShowList<T>(List<T> values)
        {
            if (values.Count == 0)
            {
                Console.WriteLine("[]");
                return;
            }
            
            Console.Write("[ ");
            for (int i = 0; i < values.Count - 1; i++) Console.Write(values[i] + "; ");
            Console.WriteLine(values.Last() + " ]");
        }

    }
}
