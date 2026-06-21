using System;
using System.Collections.Generic;
using System.IO;

namespace Vmtranslator.Parser
{
    // Comandos do VM
    public enum VmCommandType
    {
        C_ARITHMETIC,
        C_PUSH,
        C_POP,
        C_LABEL,
        C_GOTO,
        C_IF,
        C_FUNCTION,
        C_CALL,
        C_RETURN
    }

    public class Parser
    {
        private List<string> _commands;
        private int _currentIndex;
        private string[] _currentArgs;

        public Parser(string filename)
        {
            _commands = new List<string>();
            _currentIndex = 0;
            _currentArgs = Array.Empty<string>();

            string[] lines = File.ReadAllLines(filename);
            
            foreach (string line in lines)
            {
                string cleanLine = line;
                
                int commentIndex = cleanLine.IndexOf("//");
                if (commentIndex != -1)
                {
                    cleanLine = cleanLine.Substring(0, commentIndex);
                }
                
                cleanLine = cleanLine.Trim();

                if (!string.IsNullOrEmpty(cleanLine))
                {
                    _commands.Add(cleanLine);
                }
            }
        }

        public bool HasMoreCommands()
        {
            return _currentIndex < _commands.Count;
        }

        public void Advance()
        {
            if (HasMoreCommands())
            {
                string commandString = _commands[_currentIndex];
                _currentArgs = commandString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _currentIndex++;
            }
        }

        // Mapeamento de strings para os tipos do Enum
        public VmCommandType CommandType()
        {
            string cmd = _currentArgs[0].ToLower();

            if (cmd == "push") return VmCommandType.C_PUSH;
            if (cmd == "pop") return VmCommandType.C_POP;
            if (cmd == "label") return VmCommandType.C_LABEL;
            if (cmd == "goto") return VmCommandType.C_GOTO;
            if (cmd == "if-goto") return VmCommandType.C_IF;
            if (cmd == "function") return VmCommandType.C_FUNCTION;
            if (cmd == "call") return VmCommandType.C_CALL;
            if (cmd == "return") return VmCommandType.C_RETURN;
            
            return VmCommandType.C_ARITHMETIC;
        }

        public string Arg1()
        {
            if (CommandType() == VmCommandType.C_ARITHMETIC)
            {
                return _currentArgs[0];
            }
            
            return _currentArgs[1];
        }

        public int Arg2()
        {
            return int.Parse(_currentArgs[2]);
        }
    }
}