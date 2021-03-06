﻿using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace interpretator
{
    class Program
    {
        const int MAIN =   0, TASK = 1, OPERATOR = 2, TYPICAL = 3, END_SYMBOL = 4, OPEN_FIGURE = 5, //blocks opcodes 
            CLOSE_FIGURE = 6, ASSIGNER_ID = 7, ASSIGNER_OPERATOR = 10, ID = 11;

        const int BLOCK_EXPECTED = 0, OPEN_FIGURE_EXPECTED = 1, CONTENT_EXPECTED = 2,
            ID_EXPECTED = 3, END_SYMBOL_EXPECTED = 4, ASSIGNER_OPERATOR_EXPECTED = 5,
            OPERATOR_EXPECTED = 6, MATH_ID_EXPECTED = 7, OPERATOR_OR_END_SYMBOL_EXPECTED = 8;  //errors opcodes 

        const int NEW=0, OUT = 1, IN = 2, PUSH_ID_TO_STACK=3, PUSH_NUMBER_TO_STACK = 4, ADD=5, SUB=6, ASSIGNE=7; //operations opcodes 

        const int SUCCESSFULLY=1;

        private static int g_StrigOfSyntaxError;

        private static ArrayList g_VariablesList = new ArrayList();
        private static ArrayList g_Tokens = new ArrayList();
        private static ArrayList g_Lexems = new ArrayList();
        private static ArrayList g_Code = new ArrayList();
        private static ArrayList g_AllCombinations = new ArrayList(); //all combinations of code

        private static void addTokenToListTokensBeforeDelimiter(string pa_lexem, ref ArrayList pa_tokens, char pa_currentSymbol)
        {
            if (pa_lexem == "main")
                pa_tokens.Add(MAIN);
            else if (pa_lexem == "task")
            {
                pa_tokens.Add(TASK);
            }
            else if ((pa_lexem == "new") || (pa_lexem == "out") || (pa_lexem == "in"))
            {
                pa_tokens.Add(TYPICAL);
            }
            else if ((pa_lexem != "") && (pa_lexem != " "))
            {
                pa_tokens.Add(ID);
            }
            if ((pa_lexem!="") && (pa_lexem != " "))
            g_Lexems.Add(pa_lexem);
        }

        private static void addDelimiterToTokens(char pa_symbol, ref ArrayList pa_tokens)
        {
            if ((pa_symbol == '+') || (pa_symbol == '-'))
                pa_tokens.Add(OPERATOR);
            else if (pa_symbol == '{')
            {
                pa_tokens.Add(OPEN_FIGURE);
            }
            else if (pa_symbol == '}')
            {
                pa_tokens.Add(CLOSE_FIGURE);
            }
            else if (pa_symbol == '=')
            {
                pa_tokens.Add(ASSIGNER_OPERATOR);
            }
            else if (pa_symbol == ';')
            {
                pa_tokens.Add(END_SYMBOL);
            }
            if (pa_symbol != ' ')
            g_Lexems.Add(pa_symbol);
        }

        private static bool isDelimiter(char currentSymbol)
        {
            return ((currentSymbol == ' ') || (currentSymbol == ';')
                    || (currentSymbol == '{') || (currentSymbol == '}') || (currentSymbol == '+')
                || (currentSymbol == '=') || (currentSymbol == '-'));
        }

        private static ArrayList lexicAnalyze(string pa_code)
        {
            ArrayList tokens = new ArrayList();
            string lexem = "";

            for (int currentSymbolNumber = 0; currentSymbolNumber < pa_code.Length; currentSymbolNumber++)
            {
                char currentSymbol = pa_code[currentSymbolNumber];

                if (isDelimiter(currentSymbol))
                {
                    addTokenToListTokensBeforeDelimiter(lexem, ref tokens, currentSymbol);
                    addDelimiterToTokens(currentSymbol, ref tokens);
                    lexem = "";
                }
                else
                {
                    lexem += pa_code[currentSymbolNumber];
                }
            }

            int count = 0;
            for (int i=0; i<tokens.Count; i++)
            {
                if ((int)tokens[i] == ASSIGNER_OPERATOR)
                    tokens[count - 1] = ASSIGNER_ID;
                count++;
            }

            g_Tokens = tokens;
            return tokens;
        }

        private static int syntaxAnalyze(ArrayList pa_tokens)
        {
            int stationMashine = BLOCK_EXPECTED;
            int errorType = -1;

            for (int i = 0; i < pa_tokens.Count; i++)
            {
                switch (stationMashine)
                {
                    case BLOCK_EXPECTED:
                        {
                            if (((int)pa_tokens[i] == MAIN) || ((int)pa_tokens[i] == TASK))
                                stationMashine = OPEN_FIGURE_EXPECTED;
                            else
                                errorType = BLOCK_EXPECTED;
                            break;
                        }
                    case OPEN_FIGURE_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == OPEN_FIGURE)
                                stationMashine = CONTENT_EXPECTED;
                            else
                                errorType = OPEN_FIGURE_EXPECTED;
                            break;
                        }
                    case CONTENT_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == TYPICAL)
                                stationMashine = ID_EXPECTED;
                            else if ((int)pa_tokens[i] == ASSIGNER_ID)
                                stationMashine = ASSIGNER_OPERATOR_EXPECTED;
                            else if ((int)pa_tokens[i] == CLOSE_FIGURE)
                                stationMashine = BLOCK_EXPECTED;
                            else errorType = CONTENT_EXPECTED;
                            break;
                        }
                    case ID_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == ID)
                                stationMashine = END_SYMBOL_EXPECTED;
                            else
                                errorType = ID_EXPECTED;
                            break;
                        }
                    case ASSIGNER_OPERATOR_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == ASSIGNER_OPERATOR)
                                stationMashine = MATH_ID_EXPECTED;
                            else
                                errorType = ASSIGNER_OPERATOR_EXPECTED;
                            break;
                        }
                    case OPERATOR_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == OPERATOR)
                                stationMashine = MATH_ID_EXPECTED;
                            else if ((int)pa_tokens[i] == END_SYMBOL)
                                stationMashine = CONTENT_EXPECTED;
                            else
                                errorType = OPERATOR_OR_END_SYMBOL_EXPECTED;
                            break;
                        }
                    case MATH_ID_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == ID)
                                stationMashine = OPERATOR_EXPECTED;
                            else
                                errorType = MATH_ID_EXPECTED;
                            break;
                        }
                    case END_SYMBOL_EXPECTED:
                        {
                            if ((int)pa_tokens[i] == END_SYMBOL)
                                stationMashine = CONTENT_EXPECTED;
                            else
                                errorType = END_SYMBOL_EXPECTED;
                            break;
                        }
                }
                if (errorType != -1)
                {
                    g_StrigOfSyntaxError = i;
                    break;
                }
            }
            return errorType;
        }

        private static int Analyze(string pa_performingCode)
        {
            ArrayList tokens = new ArrayList();
            tokens = lexicAnalyze(pa_performingCode);
            return syntaxAnalyze(tokens);
        }
        
        private static string cutContentFromCode(string pa_code)
        {
            return pa_code.Substring(pa_code.IndexOf("{")+1, pa_code.IndexOf("}") - pa_code.IndexOf("{")-1);  
        }

        private static void cutMainBlockLemsemsAndTokens(ref ArrayList objectsFromMainBlock,bool lexemOrToken)
        {
            ArrayList objects = new ArrayList();
            if (lexemOrToken)
                objects = g_Tokens;
            else
                objects = g_Lexems;
            bool writing = false;

            if (lexemOrToken)
            foreach (int object_ in objects)
            {
                if (writing)
                    objectsFromMainBlock.Add(object_);
                if (object_ == MAIN)
                    writing = true;
                
            }else
                for (int i=0; i<objects.Count; i++)
                {
                    if (writing)
                        objectsFromMainBlock.Add(objects[i]);
                    if ((objects[i] as string) == "main")
                        writing = true;
                }
        }

        private static ArrayList getListIdentifiersFromMainBlock()
        {
            ArrayList lexemsFromMainBlock = new ArrayList();
            cutMainBlockLemsemsAndTokens(ref lexemsFromMainBlock, false);
            ArrayList tokensFromMainBlock = new ArrayList();
            cutMainBlockLemsemsAndTokens(ref tokensFromMainBlock, true);
            ArrayList identifiers = new ArrayList();
            int count = 0;
            foreach (int token in tokensFromMainBlock)
            {
                if (((int)token == ID) || ((int)token == ASSIGNER_ID))
                {
                    identifiers.Add(lexemsFromMainBlock[count]);
                }
                count++;
            }
            return identifiers;
        }

        private static string getVarName(ArrayList pa_identifiers, int variableNumber)
        {
            string varName = "";
            foreach (string var in g_VariablesList)
            {
                if (var == (string)pa_identifiers[variableNumber])
                {
                    varName = var;
                }
            }
            return varName;
        }

        private static void writeCommand(int pa_commandOpcode, ref ArrayList pa_ref_identifiers)
        {
            string varName = getVarName(pa_ref_identifiers, 0);
            int tasksCount = getTasksCount();

            if (varName != "")
            {
                ArrayList variants = new ArrayList();
                variants.Add(Convert.ToString(pa_commandOpcode) + ")" + varName + Environment.NewLine);
                g_Code.Add(variants);
            }
            else
            {
                ArrayList variants = new ArrayList();
                for (int i = 0; i < tasksCount; i++)
                {
                    variants.Add(Convert.ToString(pa_commandOpcode) + ")" + "[" + i + "]" + Environment.NewLine);
                }
                g_Code.Add(variants);
            }

            pa_ref_identifiers.RemoveAt(0);
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

        private static void treatmentMathOP(ref ArrayList pa_ref_identifiers, string pa_stringOfCode)
        {
            ArrayList operators = new ArrayList();
            int IDsCount = getIDsCountAndFillOperatorsList(pa_stringOfCode, ref operators);
            string varName;
            int tasksCount = getTasksCount();

            int addOperatorToCode = 0; //int how boolean value//0,1-don't add operator to code(false)//2-add operator to code(true) [after this variable is need to assigner (value 1)]
            for (int j=1; j<=IDsCount; j++)
            {
                ArrayList variants = new ArrayList();
                if (!IsNaN(pa_ref_identifiers[j] as string))
                {
                    variants.Add(PUSH_NUMBER_TO_STACK + ")" + pa_ref_identifiers[j] + Environment.NewLine);
                }else
                {
                    varName = getVarName(pa_ref_identifiers,1);

                    if (varName!="")
                    {
                        variants.Add(PUSH_ID_TO_STACK + ")" + pa_ref_identifiers[j] + Environment.NewLine);
                        varName = "";
                    }else
                    {
                        for (int i = 0; i < tasksCount; i++)
                        {
                            variants.Add(PUSH_NUMBER_TO_STACK + ")" + "[" + i + "]" + Environment.NewLine);
                        }
                    }
                }
                g_Code.Add(variants);

                addOperatorToCode++;

                if (addOperatorToCode==2)
                {
                    ArrayList operator_ = new ArrayList();
                    if ((char)operators[0] == '+')
                        operator_.Add(ADD + Environment.NewLine);
                    else
                        operator_.Add(SUB + Environment.NewLine);
                    operators.RemoveAt(0);
                    g_Code.Add(operator_);
                    addOperatorToCode = 1;
                }
            }

            ArrayList assigner = new ArrayList();
            assigner.Add(PUSH_ID_TO_STACK + ")" + pa_ref_identifiers[0] + Environment.NewLine);
            g_Code.Add(assigner);
            ArrayList assigneOperator = new ArrayList();
            assigneOperator.Add(ASSIGNE + Environment.NewLine);
            g_Code.Add(assigneOperator);

            int assignerCount = 1; // every math expression have one assigner
            for (int j=0; j<IDsCount+assignerCount; j++)
            {
                pa_ref_identifiers.RemoveAt(0);
            }
        }

        private static int getIDsCountAndFillOperatorsList(string pa_stringOfCode, ref ArrayList pa_operatorsList)
        {
            pa_stringOfCode = pa_stringOfCode.Replace(" ", "");
            int count = 1;
            for (int i=0; i<pa_stringOfCode.Length; i++)
            {
                if ((pa_stringOfCode[i]=='+') || (pa_stringOfCode[i] == '-'))
                {
                    pa_operatorsList.Add(pa_stringOfCode[i]);
                    count++;
                }
            }
            return count;
        }

        private static void pseudoCodeConverter(string pa_sourceCode)
        {
            string codeFromMainBlock = pa_sourceCode.Substring(pa_sourceCode.IndexOf("main"));
            string mainBlockContent = cutContentFromCode(codeFromMainBlock);
            ArrayList identifiers = new ArrayList();
            identifiers = getListIdentifiersFromMainBlock();

            while (mainBlockContent!="")
            {
                string stringOfCode = mainBlockContent.Substring(0, mainBlockContent.IndexOf(";"));

                if (stringOfCode.IndexOf("new") != -1)
                {
                    ArrayList variants = new ArrayList();
                    g_VariablesList.Add(identifiers[0]);
                    variants.Add(Convert.ToString(NEW) + ")" + identifiers[0] + Environment.NewLine);
                    g_Code.Add(variants);
                    identifiers.RemoveAt(0);
                }else if (stringOfCode.IndexOf("out") != -1)
                {
                    writeCommand(OUT,ref identifiers);
                }else if (stringOfCode.IndexOf("in") != -1)
                {
                    writeCommand(IN,ref identifiers);
                }else if (stringOfCode.IndexOf("=") != -1)
                {
                    treatmentMathOP(ref identifiers, stringOfCode);
                }

                mainBlockContent = mainBlockContent.Substring(mainBlockContent.IndexOf(";")+1);
            }

            string codeTasksBlocks= pa_sourceCode.Substring(0,pa_sourceCode.IndexOf("main"));//////////////////////////////////////////////////////////////////////////////////////////////////////
            for (int j=0; j<getTasksCount(); j++)
            {
                ArrayList begin = new ArrayList();
                begin.Add(":" + j + Environment.NewLine);
                g_Code.Add(begin);
                string taskBlockContent = cutContentFromCode(codeTasksBlocks);
                while (taskBlockContent != "")
                {
                    string stringOfCode = taskBlockContent.Substring(0, taskBlockContent.IndexOf(";"));
                    stringOfCode = stringOfCode.Replace(" ", "");

                    if (stringOfCode.IndexOf("new") != -1)
                    {
                        ArrayList variants = new ArrayList();
                        g_VariablesList.Add(stringOfCode.Replace("new",""));
                        variants.Add(Convert.ToString(NEW) + ")" + stringOfCode.Replace("new", "") + "/loc" + Environment.NewLine);
                        g_Code.Add(variants);
                    }
                    else if (stringOfCode.IndexOf("out") != -1)
                    {
                        ArrayList variants = new ArrayList();
                        variants.Add(Convert.ToString(OUT) + ")" + stringOfCode.Replace("out","") + Environment.NewLine);
                        g_Code.Add(variants);
                    }
                    else if (stringOfCode.IndexOf("in") != -1)
                    {
                        ArrayList variants = new ArrayList();
                        variants.Add(Convert.ToString(IN) + ")" + stringOfCode.Replace("in", "") + Environment.NewLine);
                        g_Code.Add(variants);
                    }
                    else if (stringOfCode.IndexOf("=") != -1)
                    {
                        
                        string identifierORnumber = "";
                        string rightPartExpression = stringOfCode.Substring(stringOfCode.IndexOf("="));

                        for (int i = 0; i < rightPartExpression.Length; i++)
                        {
                            ArrayList variants = new ArrayList();
                            if (rightPartExpression[i]=='+')
                            {
                                if (IsNaN(identifierORnumber))
                                {
                                    variants.Add(PUSH_NUMBER_TO_STACK + ")" + identifierORnumber + Environment.NewLine);
                                }else
                                {
                                    variants.Add(PUSH_ID_TO_STACK + ")" + identifierORnumber + Environment.NewLine);
                                }

                                variants.Add(Convert.ToString(ADD) + Environment.NewLine);
                                identifierORnumber = "";
                            }
                            else if (rightPartExpression[i] == '-')
                            {
                                if (IsNaN(identifierORnumber))
                                {
                                    variants.Add(PUSH_NUMBER_TO_STACK + ")" + identifierORnumber + Environment.NewLine);
                                }
                                else
                                {
                                    variants.Add(PUSH_ID_TO_STACK + ")" + identifierORnumber + Environment.NewLine);
                                }

                                identifierORnumber = "";
                                variants.Add(Convert.ToString(SUB) + Environment.NewLine);
                            }else
                            {
                                identifierORnumber += rightPartExpression[i];
                            }

                            g_Code.Add(variants);
                        }

                        ArrayList assigner = new ArrayList();
                        assigner.Add(PUSH_ID_TO_STACK + ")" + stringOfCode.Substring(0,stringOfCode.IndexOf("=")) + Environment.NewLine);
                        g_Code.Add(assigner);
                        ArrayList assigneOperator = new ArrayList();
                        assigneOperator.Add(ASSIGNE + Environment.NewLine);
                        g_Code.Add(assigneOperator);
                    }

                    taskBlockContent = taskBlockContent.Substring(taskBlockContent.IndexOf(";")+1);
                }
                ArrayList end = new ArrayList();
                end.Add("return"+Environment.NewLine);
                g_Code.Add(end);
            }
        }

        private static int getTasksCount()
        {
            int count = 0;
            foreach(int token in g_Tokens)
            {
                if (token == TASK)
                    count++;
            }
            return count;
        }

        static void Main(string[] args)
        {
            StreamReader file = new StreamReader(args[0]);
            string code = file.ReadToEnd();
            string sourceCode = code.Replace("\r", "");
            sourceCode = sourceCode.Replace("\n", "");
            int errorType = Analyze(sourceCode); //-1 - code seccesfull operation
            if (errorType == -1) 
            {
                pseudoCodeConverter(sourceCode);

                int places = 0;
                foreach (ArrayList var in g_Code)
                {
                    if (var.Count > 1)
                        places++;
                }

                ArrayList variants = new ArrayList();
                for (int i=0; i<getTasksCount(); i++)
                {
                    variants.Add(Convert.ToString(i));
                }
                generate(1,places,"",variants);

                selectionCodeForVM();
            }
        }

        private static void selectionCodeForVM()
        {
            string currentCode="";
            int successfully = 0;
            if (g_AllCombinations.Count > 0)
                for (int i = 0; i < g_AllCombinations.Count; i++)
                {
                    foreach (ArrayList stringOfCode in g_Code)
                    {
                        if (stringOfCode.Count > 1)
                        {
                            int variant = Convert.ToInt32((g_AllCombinations[i] as string).Substring(0, (g_AllCombinations[i] as string).IndexOf(" ")));
                            currentCode += stringOfCode[variant];
                            g_AllCombinations[i] = (g_AllCombinations[i] as string).Remove(0, (g_AllCombinations[i] as string).IndexOf(" ") + 1);
                        }
                        else
                        {
                            currentCode += stringOfCode[0];
                        }
                    }

                    StreamWriter file = new StreamWriter("bilded" + successfully + ".vm");
                    file.Write(currentCode);
                    file.Close();

                    /*int resultCode = 0;
                    while (resultCode==0)
                    {
                        Thread.Sleep(1000);
                        StreamReader runVMlogFile = new StreamReader("log.txt");/////////////////////////////////////////////////////////////////////////////////////////////////////
                        resultCode = Convert.ToInt32(runVMlogFile.Read());
                    }

                    if (resultCode == SUCCESSFULLY)
                        successfully++;*/

                    currentCode = "";
                }
            else
            {
                foreach (ArrayList stringOfCode in g_Code)
                {
                    currentCode += stringOfCode[0];
                }
                StreamWriter file = new StreamWriter("bilded" + successfully + ".vm");
                file.Write(currentCode);
                file.Close();
            }
        }

        private static void generate(int tmpSymbolsCount, int places, string result, ArrayList variants)
        {
            if (tmpSymbolsCount > places)
            {
                if (result!="")
                g_AllCombinations.Add(result);
                return;
            }
            foreach (string x in variants)
                generate(tmpSymbolsCount + 1, places, result + x + " ", variants);
        }
    }
}
