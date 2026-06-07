# VMTranslator - Parte 1

Projeto desenvolvido para o curso de Compiladores (Nand2Tetris - Projeto 7). Este programa traduz comandos de uma Máquina Virtual baseada em pilha (`.vm`) para a linguagem Assembly da arquitetura Hack (`.asm`), focando na implementação de comandos aritméticos e de acesso à memória.

## Nome
* Stenio Moraes Fonseca

## Linguagem e versão
* **Linguagem:** C#
* **Framework:** .NET 10.0

## Como compilar/executar

1. Certifique-se de ter o [.NET SDK 10.0](https://dotnet.microsoft.com/) instalado na sua máquina.
2. Abra o terminal na pasta raiz do projeto (onde está o arquivo `Program.cs`).
3. Execute o comando abaixo, passando o caminho do arquivo `.vm` que deseja traduzir:

```bash
dotnet run -- <caminho_do_arquivo.vm>