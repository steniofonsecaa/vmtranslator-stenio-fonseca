# VMTranslator - Máquina Virtual Hack (Nand2Tetris)

Projeto desenvolvido para a disciplina de Instrumentação (foco em sinais e ruídos) / Compiladores (Nand2Tetris - Projetos 7 e 8). Este programa traduz comandos de uma Máquina Virtual baseada em pilha (`.vm`) para a linguagem Assembly da arquitetura Hack (`.asm`), suportando arquivos únicos e diretórios inteiros.

## Nomes do aluno e linguagem utilizada
* **Desenvolvedor:** Stenio Moraes Fonseca
* **Linguagem:** C#
* **Framework:** .NET 10.0

## Funcionalidades Implementadas

### Parte 1 (Aritmética e Acesso a Memória)
* **Comandos aritméticos e lógicos:** `add`, `sub`, `neg`, `eq`, `gt`, `lt`, `and`, `or`, `not`.
* **Comandos de memória (`push` e `pop`):** Suporte aos segmentos `constant`, `local`, `argument`, `this`, `that`, `temp`, `pointer` e `static`.

### Parte 2 (Controle de Fluxo e Sub-rotinas)
* **Controle de fluxo:** `label`, `goto`, `if-goto` (com tratamento de escopo único por função `função$label`).
* **Chamadas de função:** `function`, `call`, `return` (salvamento e restauração completa do *frame* de memória na pilha).
* **Orquestrador de Diretórios:** Suporte à leitura de múltiplos arquivos `.vm` no mesmo diretório, concatenando tudo num único arquivo `.asm` de saída.
* **Bootstrap Code:** Injeção automática do código de inicialização (`SP = 256` e chamada para `Sys.init`).

## Instruções de execução

1. Certifique-se de ter o [.NET SDK 10.0](https://dotnet.microsoft.com/) instalado.
2. Abra o terminal na raiz do projeto.
3. Para compilar todos os arquivos de um diretório, execute o tradutor passando o caminho da pasta:

```bash
dotnet run -- <diretório>