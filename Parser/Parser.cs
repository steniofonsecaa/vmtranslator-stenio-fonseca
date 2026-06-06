using System;
using System.Collections.Generic;
using System.IO;

namespace Vmtranslator.Parser
{
    // Utilizaçao um Enum para representar os tipos de comando
    public enum CommandType
    {
        C_ARITHMETIC,
        C_PUSH,
        C_POP
    }

    public class Parser
    {
        private List<string> _commands;
        private int _currentIndex;
        private string[] _currentArgs;

        // Construtor: Abre o arquivo, limpa os comentários e prepara a leitura
        public Parser(string filename)
        {
            _commands = new List<string>();
            _currentIndex = 0;
            _currentArgs = Array.Empty<string>();

            // Lê todas as linhas do arquivo de uma vez
            string[] lines = File.ReadAllLines(filename);
            
            foreach (string line in lines)
            {
                string cleanLine = line;
                
                // Remove comentários a partir do "//"
                int commentIndex = cleanLine.IndexOf("//");
                if (commentIndex != -1)
                {
                    cleanLine = cleanLine.Substring(0, commentIndex);
                }
                
                // Remove espaços extras no início e no fim
                cleanLine = cleanLine.Trim();

                // Se a linha não ficou vazia, guarda na nossa lista de comandos válidos
                if (!string.IsNullOrEmpty(cleanLine))
                {
                    _commands.Add(cleanLine);
                }
            }
        }

        // Indica se há comandos pendentes
        public bool HasMoreCommands()
        {
            return _currentIndex < _commands.Count;
        }

        // Avança para o próximo comando
        public void Advance()
        {
            if (HasMoreCommands())
            {
                // Divide o comando atual em partes separadas por espaço
                string commandString = _commands[_currentIndex];
                _currentArgs = commandString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _currentIndex++;
            }
        }

        // Retorna o tipo do comando atual
        public CommandType CommandType()
        {
            string cmd = _currentArgs[0].ToLower();

            if (cmd == "push") return Parser.CommandType.C_PUSH;
            if (cmd == "pop") return Parser.CommandType.C_POP;
            
            return Parser.CommandType.C_ARITHMETIC;
        }

        // Retorna o primeiro argumento
        public string Arg1()
        {
            // Se for C_ARITHMETIC, o Arg1 é o próprio comando
            if (CommandType() == Parser.CommandType.C_ARITHMETIC)
            {
                return _currentArgs[0];
            }
            
            // Para push/pop, o Arg1 é o segmento de memória
            return _currentArgs[1];
        }

        // Retorna o índice
        public int Arg2()
        {
            return int.Parse(_currentArgs[2]);
        }
    }
}