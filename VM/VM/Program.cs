using System;
using System.Collections;
using System.IO;

namespace VM
{
    class Program
    {
        const int NEW = 0, OUT = 1, IN = 2, PUSH_ID_TO_STACK = 3, PUSH_NUMBER_TO_STACK = 4, 
            ADD = 5, SUB = 6, ASSIGNE = 7; //operations opcodes 

        const int ID=1, NUMBER=0;//opcodes type operand

        private static ArrayList g_LocalVariables = new ArrayList();
        private static Stack g_Operands = new Stack();

        static void Main(string[] args)
        {
            string code = readCodeFromFile(args[0]);
            string tasksPart="";
            if (code.IndexOf("0:")!=-1) //"0:" - label of first task
                tasksPart = cutTasksPart(ref code);
            string stringOfCode;

            while  (code!="")
            {
                stringOfCode = cutStringFromCode(ref code);

                if (stringOfCode.IndexOf(")")!=-1) 
                {
                    int operation = stringOfCode[0] - 48; //0 symbol - opcode operation
                    string operand = stringOfCode.Substring(stringOfCode.IndexOf(")") + ")".Length);

                    if (stringOfCode.IndexOf("[")!=-1) //example: 4)[0] explain: "4"-opcode operation ")"-delimiter "[0]" - number of task 
                    {
                        operand = operand.Replace("[", "");
                        operand = operand.Replace("]", "");
                        int result=perfomTask(Convert.ToInt32(operand), tasksPart);
                    }
                    else //example: 3)a explain: "3"-opcode operation ")"-delimiter "a" - name of variable
                    {

                    }
                }else
                {

                }
            }
        }

        private static string cutTasksPart(ref string pa_code)
        {
            string tasksPart = pa_code.Substring(pa_code.IndexOf("0:"));
            pa_code = pa_code.Substring(0,pa_code.IndexOf("0:"));
            return tasksPart;
        }

        private static int perfomTask(int pa_numberTask, string pa_tasksCode)
        {
            int taskLabelIndex = pa_tasksCode.IndexOf(Convert.ToString(pa_numberTask) + ":");
            string taskContent = pa_tasksCode.Substring(taskLabelIndex + "0:\r\n".Length);
            string stringOfCode;
            int taskValue=0;

            while (true)
            {
                stringOfCode = cutStringFromCode(ref taskContent);

                if (stringOfCode.IndexOf(Convert.ToString(NEW)) != -1)
                {
                    string operand = stringOfCode.Substring(stringOfCode.IndexOf(")") + ")".Length);
                    string varName = operand.Substring(0, operand.IndexOf("/"));
                    Variable newVar = new Variable(varName, 0);
                    g_LocalVariables.Add(newVar);
                }else if (stringOfCode.IndexOf(")")!=-1)
                {
                    int operation = stringOfCode[0] - 48; //0 symbol - opcode operation
                    string operand = stringOfCode.Substring(stringOfCode.IndexOf(")") + ")".Length);

                    switch (operation)
                    {
                        case OUT:
                            {
                                if (IsNaN(operand))
                                {
                                    foreach (Variable var in g_LocalVariables)
                                    {
                                        if (var.name==operand)
                                        {
                                            taskValue = var.value;
                                            break;
                                        }
                                    }
                                }else
                                {
                                    taskValue=Convert.ToInt32(operand);
                                }
                                break;
                            }
                        case IN:
                            {
                                    foreach (Variable var in g_LocalVariables)
                                    {
                                        if (var.name == operand)
                                        {
                                            var.value = Convert.ToInt32(Console.ReadLine());
                                            break;
                                        }
                                    }
                                break;
                            }
                        case PUSH_ID_TO_STACK:
                            {
                                foreach (Variable var in g_LocalVariables)
                                {
                                    if (var.name == operand)
                                    {
                                        Wrap wrap = new Wrap(ID, var.value, operand);
                                        g_Operands.Push(wrap);
                                        break;
                                    }
                                }
                                break;
                            }
                        case PUSH_NUMBER_TO_STACK:
                            {
                                int value = Convert.ToInt32(operand);
                                Wrap wrap = new Wrap(NUMBER, value, "");
                                g_Operands.Push(wrap);
                                break;
                            }
                    }
                }else
                {
                    if (IsNaN(stringOfCode))
                    {
                        return taskValue;
                    }else
                    {
                        int operation = stringOfCode[0] - 48; //0 symbol - opcode operation
                        switch (operation)
                        {
                            case ADD:
                                {
                                    int operand1 = (g_Operands.Pop() as Wrap).number;
                                    int operand2 = (g_Operands.Pop() as Wrap).number;
                                    g_Operands.Push(operand1 + operand2);
                                    break;
                                }
                            case SUB:
                                {
                                    int operand1 = (g_Operands.Pop() as Wrap).number;
                                    int operand2 = (g_Operands.Pop() as Wrap).number;
                                    g_Operands.Push(operand2 - operand1);
                                    break;
                                }
                            case ASSIGNE:
                                {
                                    string assignerId = (g_Operands.Pop() as Wrap).varName;
                                    int operand = Convert.ToInt32(g_Operands.Pop());
                                    foreach (Variable var in g_LocalVariables)
                                    {
                                        if (var.name==assignerId)
                                        {
                                            var.value = operand;
                                            break;
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }      
            }
        }

        private static bool IsNaN(string value)
        {
            if (value[0] != '0')
                for (int i = 0; i < value.Length; i++)
                {
                    if ((value[i] < 48) || (value[i] > 57))
                    {
                        return true;
                    }
                }
            else
                return true;
            return false;
        }

        private static string cutStringFromCode(ref string pa_ref_code)
        {
            string stringOfCode = pa_ref_code.Substring(0, pa_ref_code.IndexOf("\r\n"));
            pa_ref_code = pa_ref_code.Remove(0, pa_ref_code.IndexOf("\r\n") + 2);
            return stringOfCode;
        }

        private static string readCodeFromFile(string pa_fileName)
        {
            StreamReader codeFile = new StreamReader(pa_fileName);
            string code;
            code = codeFile.ReadToEnd();
            codeFile.Close();
            return code;
        }
    }

    class Wrap
    {
        public readonly int type;//0 - number, 1 - id
        public string varName;
        public int number;

        public Wrap(int pa_type, int pa_number, string pa_name)
        {
            type = pa_type;
            number = pa_number;
            varName = pa_name;
        }     
    }

    class Variable
    {
        public readonly string name;
        public int value;
        public Variable(string pa_name, int pa_value)
        {
            name = pa_name;
            value = pa_value;
        }
    }
}
