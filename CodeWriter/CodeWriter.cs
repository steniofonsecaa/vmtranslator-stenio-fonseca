using System;
using System.IO;

namespace Vmtranslator.CodeWriter
{
    public class CodeWriter
    {
        private StreamWriter _writer;
        private string _filename;
        private int _jumpCounter; // Necessário para gerar rótulos únicos em eq, gt, lt
        private string _currentFunction = "null";

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

        // Gera código para empilhar valores (PUSH)
        public void WritePush(string segment, int index)
        {
            _writer.WriteLine($"// push {segment} {index}");

            if (segment == "constant")
            {
                // constant i: *SP = i, SP++
                _writer.WriteLine($"@{index}");
                _writer.WriteLine("D=A");
            }
            else if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
            {
                // addr = segmentPointer + i, *SP = *addr, SP++
                string symbol = GetSegmentSymbol(segment);
                _writer.WriteLine($"@{index}");
                _writer.WriteLine("D=A");
                _writer.WriteLine($"@{symbol}");
                _writer.WriteLine("A=M+D"); // Vai para o endereço base + índice
                _writer.WriteLine("D=M");   // Pega o valor do endereço e guarda em D
            }
            else if (segment == "temp")
            {
                // addr = 5 + i, *SP = *addr, SP++
                int addr = 5 + index;
                _writer.WriteLine($"@{addr}");
                _writer.WriteLine("D=M");
            }
            else if (segment == "pointer")
            {
                // pointer 0 = THIS (3), pointer 1 = THAT (4)
                int addr = 3 + index;
                _writer.WriteLine($"@{addr}");
                _writer.WriteLine("D=M");
            }
            else if (segment == "static")
            {
                // Usa o nome do arquivo para garantir escopo estático
                _writer.WriteLine($"@{_filename}.{index}");
                _writer.WriteLine("D=M");
            }

            // Bloco comum a todos os PUSH: Põe o valor guardado em 'D' no topo da pilha e avança SP
            _writer.WriteLine("@SP");
            _writer.WriteLine("A=M");
            _writer.WriteLine("M=D");
            _writer.WriteLine("@SP");
            _writer.WriteLine("M=M+1");
        }

        // Gera código para desempilhar valores (POP)
        public void WritePop(string segment, int index)
        {
            _writer.WriteLine($"// pop {segment} {index}");

            if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
            {
                // addr = segmentPointer + i, SP--, *addr = *SP
                string symbol = GetSegmentSymbol(segment);
                
                // Calcula o endereço destino e guarda no registo temporário R13
                _writer.WriteLine($"@{index}");
                _writer.WriteLine("D=A");
                _writer.WriteLine($"@{symbol}");
                _writer.WriteLine("D=M+D"); // D = base + índice
                _writer.WriteLine("@R13");
                _writer.WriteLine("M=D");   // R13 guarda o endereço de destino

                // Tira o valor do topo da pilha e guarda em D
                _writer.WriteLine("@SP");
                _writer.WriteLine("AM=M-1");
                _writer.WriteLine("D=M");

                // Vai para o endereço guardado em R13 e escreve o valor
                _writer.WriteLine("@R13");
                _writer.WriteLine("A=M");
                _writer.WriteLine("M=D");
            }
            else if (segment == "temp" || segment == "pointer")
            {
                // Temp e Pointer são blocos fixos, não precisam do R13
                int addr = (segment == "temp") ? 5 + index : 3 + index;
                
                _writer.WriteLine("@SP");
                _writer.WriteLine("AM=M-1");
                _writer.WriteLine("D=M"); // D guarda o valor desempilhado
                
                _writer.WriteLine($"@{addr}");
                _writer.WriteLine("M=D"); // Escreve direto no endereço
            }
            else if (segment == "static")
            {
                _writer.WriteLine("@SP");
                _writer.WriteLine("AM=M-1");
                _writer.WriteLine("D=M");
                
                _writer.WriteLine($"@{_filename}.{index}");
                _writer.WriteLine("M=D");
            }
        }

        // Método auxiliar para traduzir o nome do segmento para o símbolo do Assembly
        private string GetSegmentSymbol(string segment)
        {
            if (segment == "local") return "LCL";
            if (segment == "argument") return "ARG";
            if (segment == "this") return "THIS";
            if (segment == "that") return "THAT";
            return "";
        }

        // Gera código para definir um rótulo (label)
        public void WriteLabel(string label)
        {
            _writer.WriteLine($"({_currentFunction}${label})");
        }

        // Gera código para salto incondicional (goto)
        public void WriteGoto(string label)
        {
            _writer.WriteLine($"@{_currentFunction}${label}");
            _writer.WriteLine("0;JMP");
        }
        
        // Gera código para salto condicional (if-goto)
        public void WriteIf(string label)
        {
            _writer.WriteLine("@SP");
            _writer.WriteLine("AM=M-1");
            _writer.WriteLine("D=M");
            _writer.WriteLine($"@{_currentFunction}${label}");
            _writer.WriteLine("D;JNE");
        }

        // Escreve o código de uma função (function)
        public void WriteFunction(string functionName, int numLocals)
        {
            // Atualiza o escopo para a nova função (usado nos labels)
            _currentFunction = functionName;

            // Escreve o rótulo de entrada da função
            _writer.WriteLine($"({functionName})");

            // Empilha '0' para cada variável local (inicialização)
            for (int i = 0; i < numLocals; i++)
            {
                _writer.WriteLine("@SP");
                _writer.WriteLine("A=M");
                _writer.WriteLine("M=0");
                
                _writer.WriteLine("@SP");
                _writer.WriteLine("M=M+1");
            }
        }

        // Escreve o comando de retorno
        public void WriteReturn()
        {
            // endFrame = LCL (Salva LCL em R14 temporariamente)
            _writer.WriteLine("@LCL");
            _writer.WriteLine("D=M");
            _writer.WriteLine("@R14");
            _writer.WriteLine("M=D");

            // retAddr = *(endFrame - 5) (Salva o endereço de retorno em R15)
            _writer.WriteLine("@5");
            _writer.WriteLine("A=D-A"); 
            _writer.WriteLine("D=M");
            _writer.WriteLine("@R15");
            _writer.WriteLine("M=D");

            // *ARG = pop() (Coloca o valor de retorno no topo do chamador)
            _writer.WriteLine("@SP");
            _writer.WriteLine("AM=M-1");
            _writer.WriteLine("D=M");
            _writer.WriteLine("@ARG");
            _writer.WriteLine("A=M");
            _writer.WriteLine("M=D");

            // SP = ARG + 1 (Restaura o topo da pilha para logo após o valor de retorno)
            _writer.WriteLine("@ARG");
            _writer.WriteLine("D=M+1");
            _writer.WriteLine("@SP");
            _writer.WriteLine("M=D");

            // Restaura THAT, THIS, ARG, LCL do frame do chamador
            // Decrementando R14 a cada passo para varrer o frame de trás para frente
            _writer.WriteLine("@R14");
            _writer.WriteLine("AM=M-1"); // endFrame - 1
            _writer.WriteLine("D=M");
            _writer.WriteLine("@THAT");
            _writer.WriteLine("M=D");

            _writer.WriteLine("@R14");
            _writer.WriteLine("AM=M-1"); // endFrame - 2
            _writer.WriteLine("D=M");
            _writer.WriteLine("@THIS");
            _writer.WriteLine("M=D");

            _writer.WriteLine("@R14");
            _writer.WriteLine("AM=M-1"); // endFrame - 3
            _writer.WriteLine("D=M");
            _writer.WriteLine("@ARG");
            _writer.WriteLine("M=D");

            _writer.WriteLine("@R14");
            _writer.WriteLine("AM=M-1"); // endFrame - 4
            _writer.WriteLine("D=M");
            _writer.WriteLine("@LCL");
            _writer.WriteLine("M=D");

            // goto retAddr (Salta para o endereço salvo em R15)
            _writer.WriteLine("@R15");
            _writer.WriteLine("A=M");
            _writer.WriteLine("0;JMP");
        }

        // Finaliza e fecha o arquivo
        public void Close()
        {
            _writer.Close();
        }
    }
}