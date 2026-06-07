using System;
using System.IO;
using Vmtranslator.Parser;
using Vmtranslator.CodeWriter;

// Validação da entrada
if (args.Length == 0)
{
    Console.WriteLine("Erro: Por favor, forneça o caminho do arquivo .vm.");
    Console.WriteLine("Uso: dotnet run -- <caminho_do_arquivo.vm>");
    return;
}

string inputPath = args[0];

if (!File.Exists(inputPath))
{
    Console.WriteLine($"Erro: O arquivo '{inputPath}' não foi encontrado.");
    return;
}

// Geração do nome do arquivo de saída (.asm)
// Aqui substituimos a extensão .vm por .asm de forma segura
string outputPath = Path.ChangeExtension(inputPath, ".asm");

Console.WriteLine($"Iniciando tradução de: {Path.GetFileName(inputPath)}");

// Inicialização dos módulos
Parser parser = new Parser(inputPath);
CodeWriter codeWriter = new CodeWriter(outputPath);

// 4. O Loop Principal
while (parser.HasMoreCommands())
{
    parser.Advance();
    
    VmCommandType type = parser.CommandType();

    switch (type)
    {
        case VmCommandType.C_ARITHMETIC:
            codeWriter.WriteArithmetic(parser.Arg1());
            break;
            
        case VmCommandType.C_PUSH:
            codeWriter.WritePush(parser.Arg1(), parser.Arg2());
            break;
            
        case VmCommandType.C_POP:
            codeWriter.WritePop(parser.Arg1(), parser.Arg2());
            break;
    }
}

// Fechamento e finalização
codeWriter.Close();

Console.WriteLine($"Tradução concluída com sucesso! Arquivo gerado: {Path.GetFileName(outputPath)}");