using System;
using System.IO;

namespace Vmtranslator.CodeWriter
{
    public class CodeWriter
    {
        private StreamWriter _writer;
        private string _filename;
        private int _jumpCounter; // Necessário para gerar rótulos únicos em eq, gt, lt

        // Construtor: Abre arquivo .asm para escrita
        public CodeWriter(string outputPath)
        {
            _writer = new StreamWriter(outputPath);
            // Guarda o nome do ficheiro (sem o caminho ou extensão) para usar no segmento 'static' depois
            _filename = Path.GetFileNameWithoutExtension(outputPath);
            _jumpCounter = 0;
        }

        // Gera código Assembly para os comandos matemáticos e lógicos
        public void WriteArithmetic(string cmd)
        {
            // Somente para debug: escreve o comando VM original como comentário no Assembly
            _writer.WriteLine($"// {cmd}");

            if (cmd == "add" || cmd == "sub" || cmd == "and" || cmd == "or")
            {
                // Operações com DOIS operandos
                _writer.WriteLine("@SP");
                _writer.WriteLine("AM=M-1"); // Decrementa SP e vai para o topo (y)
                _writer.WriteLine("D=M");    // Guarda y em D
                _writer.WriteLine("A=A-1");  // Vai para o valor anterior (x)

                if (cmd == "add") _writer.WriteLine("M=M+D"); // x = x + y
                if (cmd == "sub") _writer.WriteLine("M=M-D"); // x = x - y
                if (cmd == "and") _writer.WriteLine("M=M&D"); // x = x & y
                if (cmd == "or")  _writer.WriteLine("M=M|D"); // x = x | y
            }
            else if (cmd == "neg" || cmd == "not")
            {
                // Operações com UM operando
                _writer.WriteLine("@SP");
                _writer.WriteLine("A=M-1"); // Vai para o topo

                if (cmd == "neg") _writer.WriteLine("M=-M"); // inverte o sinal
                if (cmd == "not") _writer.WriteLine("M=!M"); // nega bit a bit
            }
            else if (cmd == "eq" || cmd == "gt" || cmd == "lt")
            {
                // Operações Condicionais: Necessitam de JUMPs
                string jumpTrue = $"JUMP_TRUE_{_jumpCounter}";
                string jumpEnd = $"JUMP_END_{_jumpCounter}";
                _jumpCounter++;

                _writer.WriteLine("@SP");
                _writer.WriteLine("AM=M-1"); // Pega y
                _writer.WriteLine("D=M");
                _writer.WriteLine("A=A-1");  // Pega x
                _writer.WriteLine("D=M-D");  // Calcula x - y

                // Decide qual o tipo de salto
                _writer.WriteLine($"@{jumpTrue}");
                if (cmd == "eq") _writer.WriteLine("D;JEQ");
                if (cmd == "gt") _writer.WriteLine("D;JGT");
                if (cmd == "lt") _writer.WriteLine("D;JLT"); 

                // Se for FALSO (não saltou), escreve 0
                _writer.WriteLine("@SP");
                _writer.WriteLine("A=M-1");
                _writer.WriteLine("M=0");
                _writer.WriteLine($"@{jumpEnd}");
                _writer.WriteLine("0;JMP");

                // Se for VERDADEIRO (saltou), escreve -1 (true no Hack)
                _writer.WriteLine($"({jumpTrue})");
                _writer.WriteLine("@SP");
                _writer.WriteLine("A=M-1");
                _writer.WriteLine("M=-1");

                // Fim da condição
                _writer.WriteLine($"({jumpEnd})");
            }
        }

        // Esboço do método de Push/Pop
        public void WritePushPop(string commandType, string segment, int index)
        {
            // Código para debug: escreve o comando VM original como comentário no Assembly
        }

        // Finaliza e fecha o arquivo
        public void Close()
        {
            _writer.Close();
        }
    }
}