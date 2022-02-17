using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using JASON_Compiler;

public enum Token_Class
{
    Main, Int, Flaot, String, Read, Write, Repeat, Until, If,
    Else, Elseif, then, Return, Endl, Semicolon, Comma,
    LParanthesis, RParanthesis, ConditionOp, ArithmaticOp,
    Number, Comment, Identifier, AssignmentOp, BooleanOp, RCurlyParanthesis,
    LCurlyParanthesis, Datatype, End, Dot, AddOp,MinusOp,MultOp,DivideOp
}
namespace JASON_Compiler
{

    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Datatype);
            ReservedWords.Add("float", Token_Class.Datatype);
            ReservedWords.Add("string", Token_Class.Datatype);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("elseif", Token_Class.Elseif);
            ReservedWords.Add("then", Token_Class.then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("main", Token_Class.Main);

            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add(":=", Token_Class.AssignmentOp);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("}", Token_Class.RCurlyParanthesis);
            Operators.Add("{", Token_Class.LCurlyParanthesis);
            Operators.Add("=", Token_Class.ConditionOp);
            Operators.Add("<", Token_Class.ConditionOp);
            Operators.Add(">", Token_Class.ConditionOp);
            Operators.Add("<>", Token_Class.ConditionOp);
            Operators.Add("+", Token_Class.AddOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("||", Token_Class.BooleanOp);
            Operators.Add("&&", Token_Class.BooleanOp);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                string CurrentLexeme = "";

                if (SourceCode[i] == ' ' || SourceCode[i] == '\r' || SourceCode[i] == '\n' || SourceCode[i] == '\t')
                    continue;

                if (char.IsLetter(SourceCode[i]))
                {
                    while (j < SourceCode.Length && char.IsLetterOrDigit(SourceCode[j]))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                }
                else if (char.IsDigit(SourceCode[i]))
                {
                    while (j < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[j]) || SourceCode[j] == '.'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                }
                else if (i < SourceCode.Length - 1 && SourceCode[i] == '/' && SourceCode[i + 1] == '*')
                {
                    while (j < SourceCode.Length - 1)
                    {

                        CurrentLexeme += SourceCode[j];
                        j++;

                        if (SourceCode[j] == '/' && SourceCode[j - 1] == '*')
                            break;
                    }
                    CurrentLexeme += SourceCode[j];
                    i = j + 1;
                }
                else if (SourceCode[i] == '\"')
                {
                    CurrentLexeme += SourceCode[j];
                    j++;
                    while (j < SourceCode.Length && SourceCode[j] != '\"')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    if (j < SourceCode.Length)
                        CurrentLexeme += SourceCode[j];
                    i = j;
                }
                else if (SourceCode[i] == '(' || SourceCode[i] == ')' || SourceCode[i] == '{' || SourceCode[i] == '}')
                {
                    CurrentLexeme += SourceCode[i];
                }
                else if (!char.IsLetterOrDigit(SourceCode[i]))
                {
                    while (j < SourceCode.Length && (SourceCode[j] != ' ' && !char.IsLetterOrDigit(SourceCode[j])))
                    {
                        CurrentLexeme += SourceCode[j];
                        if (j < SourceCode.Length - 2)
                            if (SourceCode[j] == '<' && SourceCode[j + 1] == '>')
                            {
                                CurrentLexeme += SourceCode[j + 1];
                                j += 2;
                                break;
                            }
                            else if (Operators.ContainsKey(CurrentLexeme))
                            {
                                j++;
                                break;
                            }
                        j++;
                    }
                    i = j - 1;
                }

                FindTokenClass(CurrentLexeme);
            }
            JASON_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
                Tokens.Add(Tok);
            }
            else if (isNumber(Lex))
            {
                Tok.token_type = Token_Class.Number;
                Tokens.Add(Tok);
            }
            else if (isComment(Lex.Replace("\r\n", "").Replace("\t", "").Replace(" ", "")))
            {
                Tok.token_type = Token_Class.Comment;
                Tokens.Add(Tok);
            }
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.String;
                Tokens.Add(Tok);
            }
            else
            {
                Errors.Error_List.Add("-Unrecognized token: " + Lex);
            }
        }

        bool isIdentifier(string lex)
        {
            return new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled).IsMatch(lex);
        }
        bool isNumber(string lex)
        {
            return new Regex(@"^[0-9]+(\.[0-9]*)?$", RegexOptions.Compiled).IsMatch(lex);
        }
        bool isComment(string lex)
        {
            return new Regex(@"^/\*(.*|\s)\*/$", RegexOptions.Compiled).IsMatch(lex);
        }
        bool isString(string lex)
        {
            return new Regex("^\".*\"$", RegexOptions.Compiled).IsMatch(lex);
        }
    }
}

