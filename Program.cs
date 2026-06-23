using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Vmtranslator.Parser;
using Vmtranslator.CodeWriter;

// Validação da entrada
if (args.Length == 0)
{
    Console.WriteLine("Erro: Por favor, forneça o caminho de um arquivo .vm ou de um diretório.");
    Console.WriteLine("Uso: dotnet run -- <caminho>");
    return;
}

string inputPath = args[0];
List<string> vmFiles = new List<string>();
string outputPath = "";

// Verifica se a entrada é um diretório ou um arquivo único
if (Directory.Exists(inputPath))
{
    // Se for pasta, pega todos os arquivos .vm lá dentro
    vmFiles = Directory.GetFiles(inputPath, "*.vm").ToList();
    
    // O nome do arquivo de saída .asm será o próprio nome da pasta
    string dirName = new DirectoryInfo(inputPath).Name;
    outputPath = Path.Combine(inputPath, $"{dirName}.asm");
}
else if (File.Exists(inputPath) && inputPath.EndsWith(".vm"))
{
    // Se for arquivo único, o processo continua como na Parte 1
    vmFiles.Add(inputPath);
    outputPath = Path.ChangeExtension(inputPath, ".asm");
}
else
{
    Console.WriteLine($"Erro: O caminho '{inputPath}' não é um arquivo .vm nem um diretório válido.");
    return;
}

if (vmFiles.Count == 0)
{
    Console.WriteLine("Nenhum arquivo .vm encontrado no diretório especificado.");
    return;
}

Console.WriteLine($"Iniciando compilação. Arquivo de saída: {Path.GetFileName(outputPath)}");

// Inicialização do CodeWriter e execução do Bootstrap
CodeWriter codeWriter = new CodeWriter(outputPath);
codeWriter.WriteInit();

// Processa cada arquivo .vm encontrado na lista
foreach (string vmFile in vmFiles)
{
    Console.WriteLine($"Processando: {Path.GetFileName(vmFile)}");
    
    // Informa ao CodeWriter qual é o arquivo atual (para a regra do segmento static)
    codeWriter.SetFileName(Path.GetFileNameWithoutExtension(vmFile));
    
    Parser parser = new Parser(vmFile);

    while (parser.HasMoreCommands())
    {
        parser.Advance();
        VmCommandType type = parser.CommandType();

        // O novo Switch contemplando todos os comandos da linguagem
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
            case VmCommandType.C_LABEL:
                codeWriter.WriteLabel(parser.Arg1());
                break;
            case VmCommandType.C_GOTO:
                codeWriter.WriteGoto(parser.Arg1());
                break;
            case VmCommandType.C_IF:
                codeWriter.WriteIf(parser.Arg1());
                break;
            case VmCommandType.C_FUNCTION:
                codeWriter.WriteFunction(parser.Arg1(), parser.Arg2());
                break;
            case VmCommandType.C_CALL:
                codeWriter.WriteCall(parser.Arg1(), parser.Arg2());
                break;
            case VmCommandType.C_RETURN:
                codeWriter.WriteReturn();
                break;
        }
    }
}

// Finaliza e fecha o arquivo
codeWriter.Close();

Console.WriteLine("Tradução da Parte 2 concluída com sucesso!");