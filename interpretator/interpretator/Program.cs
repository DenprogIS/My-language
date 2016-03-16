using System;
using System.Collections;
using System.IO;

namespace interpretator
{
    class Program
    {
        const int MAIN = 0, TASK = 1, OPERATOR = 2, TYPICAL = 3, END_SYMBOL = 4, OPEN_FIGURE = 5,
            CLOSE_FIGURE = 6, ASSIGNER_ID = 7, START = 8, STOP = 9, ASSIGNER_OPERATOR = 10, ID = 11;

        const int BLOCK_EXPECTED = 0, OPEN_FIGURE_EXPECTED = 1, CONTENT_EXPECTED = 2,
            ID_EXPECTED = 3, END_SYMBOL_EXPECTED = 4, ASSIGNER_OPERATOR_EXPECTED = 5,
            OPERATOR_EXPECTED = 6, MATH_ID_EXPECTED = 7, OPERATOR_OR_END_SYMBOL_EXPECTED = 8;

        private static int g_StrigOfSyntaxError;

        private static ArrayList g_VariablesList = new ArrayList();

        private static ArrayList g_Tokens = new ArrayList();

        private static ArrayList g_Lexems = new ArrayList();

        private static ArrayList g_variants = new ArrayList();

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
            else if (pa_lexem == "start")
            {
                pa_tokens.Add(START);
            }
            else if (pa_lexem == "stop")
            {
                pa_tokens.Add(STOP);
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
            foreach (int token in tokens)
            {
                if (token == ASSIGNER_OPERATOR)
                    break;

                count++;
            }
            tokens[count - 1] = ASSIGNER_ID;

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
        
        private static string insertContentFromCode(string pa_code)
        {
            return pa_code.Substring(pa_code.IndexOf("{")+1, pa_code.IndexOf("}") - pa_code.IndexOf("{")-1);  
        }

        private static ArrayList getListIdentifiersFromCode()
        {
            ArrayList identifiers = new ArrayList();
            int count = 0;
            foreach (int token in g_Tokens)
            {
                if ((int)token == ID)
                {
                    identifiers.Add(g_Lexems[count]);
                }
                count++;
            }
            return identifiers;
        }

        /*private static void Convert(int pa_threadIndex, string pa_stringOfCode)
        {
            ArrayList identifiers = new ArrayList();
            identifiers = getListIdentifiersFromCode();

            if (pa_stringOfCode.IndexOf("new") != -1)
            {
                g_VariablesList.Add(identifiers[0]);
                identifiers.RemoveAt(0);
                (g_CodeThreads[pa_threadIndex] as string) += "new " + 
            }
        }*/

        private static void pseudoCodeConverter(string pa_sourceCode)
        {
           while (pa_sourceCode!="")
            {
                ArrayList identifiers = new ArrayList();
                identifiers = getListIdentifiersFromCode();

                string content = insertContentFromCode(pa_sourceCode);
                string stringOfCode = content.Substring(0, content.IndexOf(";"));
                string publicCode = "";

                if (stringOfCode.IndexOf("new") != -1)
                {
                    g_VariablesList.Add(identifiers[0]);
                    publicCode += identifiers[0] + Environment.NewLine;
                    identifiers.RemoveAt(0);
                }else if (stringOfCode.IndexOf("out") != -1)
                {
                    for(int i=0; i<g_variants.Count; i++)
                    {
                        g_variants[i]+= "out a" + Environment.NewLine;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            StreamReader file = new StreamReader(args[0]);
            string code = file.ReadToEnd();
            string sourceCode = code.Replace("\r", "");
            sourceCode = sourceCode.Replace("\n", "");
            if (Analyze(sourceCode) == -1) //-1 - code seccesfull operation
            {
                pseudoCodeConverter(sourceCode);
            }
        }
    }
}
