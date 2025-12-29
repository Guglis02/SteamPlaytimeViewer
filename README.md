# Steam Playtime Viewer

Um aplicativo de TUI para visualizar e filtrar seus jogos do Steam com informações detalhadas de tempo de jogo, conquistas e histórico de sessões.

## 📋 Descrição

O Steam Playtime Viewer é uma ferramenta que permite:

- **Visualizar bibliotecas de jogos** de múltiplos usuários Steam (desde que essa informação seja pública no perfil) em uma tabela
- **Sincronizar dados** tanto pela API oficial do Steam quanto por arquivos VDF locais (é mais poderoso pois pega jogos da família, mas requer login da conta na máquina local)
- **Filtrar jogos** por título
- **Ordenar jogos** por diferentes critérios (título, tempo de jogo, conquistas, etc.)
- **Gerenciar múltiplos usuários** locais ou remotos

## 🎮 Características

- ✅ Interface no terminal com navegação por setas
- ✅ Suporte para múltiplos usuários Steam
- ✅ Sincronização com API Steam (requer API Key)
- ✅ Sincronização com arquivos VDF locais (requer login da conta na máquina local)
- ✅ Múltiplas opções de ordenação
- ✅ Sistema de comandos extensível

## 🛠️ Build e Execução

### Pré-requisitos

- **.NET 10.0** ou superior
- **Steam API Key**

### Compilar

```bash
# Debug build
dotnet build

# Release build (otimizado)
dotnet build -c Release
```

### Executar

```bash
# Run direto
dotnet run

# Usar o executável compilado
./bin/Debug/net10.0/SteamPlaytimeViewer
```

## ⚙️ Configuração

### Arquivo de Configuração

Crie um arquivo `appsettings.Secret.json` na raiz do projeto:

```json
{
  "SteamSettings": {
    "ApiKey": "YOUR_STEAM_API_KEY_HERE"
  }
}
```

**Obter sua Steam API Key:**
1. Acesse: https://steamcommunity.com/dev/apikey
2. Faça login com sua conta Steam
3. Copie a chave exibida

### Banco de Dados

O aplicativo cria automaticamente um banco de dados SQLite (`steamdata.db`) na pasta do projeto na primeira execução.

## ⌨️ Manual de Comandos

### Navegação Básica

- **Seta para cima** (`↑`) - Rolar tabela para cima
- **Seta para baixo** (`↓`) - Rolar tabela para baixo
- **Enter** - Executar comando

### Comandos Disponíveis

#### `help`
Mostra lista de todos os comandos disponíveis ou detalhes sobre um comando específico.

**Uso:**
```
help                  # Lista todos os comandos
help <comando>        # Mostra detalhes de um comando específico
```

**Exemplos:**
```
help
help sort
help user
```

---

#### `user <username|steamid>`
Muda o usuário a ser exibido na tabela. Suporta username ou SteamID64.

**Parâmetros:**
- `<username>` - Nome do perfil (Só funciona para usuários já sincronizados).
- `<steamid>` - SteamID64 numérico.

**Comportamento:**
- Se usar **username**: busca no banco de dados local. Se não encontrado, mostra erro
- Se usar **SteamID**: busca no banco. Se não existir:
  - Consulta a API Steam e busca o nome de perfil
  - Registra o novo usuário no banco

**Exemplos:**
```
user hyan
user 76561198062983485
user "nome com espaço"
```

---

#### `sync account|local`
Sincroniza dados de jogos do usuário atual. Oferece duas estratégias.

**Parâmetros:**
- `account` - Sincroniza pela API oficial do Steam
- `local` - Sincroniza pelo arquivo VDF local da Steam (Também usa a API para pegar dados complementares de achievements)

**Diferenças:**
| Estratégia | Requisitos | Vantagens | Limitações |
|------------|-----------|----------|-----------|
| **account** | API Key + Acesso público | Funciona para contas remotas | Não pega jogos do Family Share, jogos devem ser públicos |
| **local** | API Key + Steam instalado no PC | Funciona mesmo com jogos privados | Apenas contas que logaram no PC |

**Exemplos:**
```
sync account           # Sincroniza pela conta Steam
sync local             # Sincroniza do arquivo VDF local
```

---

#### `search <termo>`
Filtra os jogos exibidos por título. Suporta busca parcial.

**Parâmetros:**
- `<termo>` - Texto para buscar no título dos jogos (case-insensitive)
- Sem parâmetros: remove o filtro

**Comportamento:**
- Busca é parcial (ex: "dark" encontra "Dark Souls", "Darkest Dungeon")

**Exemplos:**
```
search dark             # Busca jogos com "dark" no título
search                  # Remove filtro
```

---

#### `folder [caminho]`
Define ou exibe a pasta de instalação do Steam no computador.

**Parâmetros:**
- `[caminho]` (opcional) - Caminho completo da pasta Steam
  - Sem parâmetros: mostra o caminho atual configurado

**Comportamento:**
- Normaliza automaticamente separadores (`/` e `\`) para o SO em uso
- Suporta variáveis de ambiente (`%USERPROFILE%` no Windows, `$HOME` no Linux)
- Converte caminhos relativos para caminhos absolutos
- Remove aspas se o caminho for colado entre aspas
- Valida se o diretório existe antes de salvar
- Necessário para usar o comando `sync local`

**Localização padrão por SO:**

| Sistema | Caminho Padrão |
|---------|----------------|
| Windows | `C:\Program Files (x86)\Steam` |
| Linux | `~/.local/share/Steam` ou `~/.steam/steam` |
| macOS | `~/Library/Application Support/Steam` |

**Exemplos:**
```
folder # Mostra caminho atual

folder C:\Program Files (x86)\Steam # Windows (com backslash)
folder "C:/Program Files (x86)/Steam" # Windows (com forward slash)
folder %PROGRAMFILES(X86)%\Steam # Windows (com variável)

folder /home/usuario/.local/share/Steam # Linux (caminho absoluto)
folder ~/.local/share/Steam # Linux (com ~)
folder /var/app/com.valvesoftware.Steam/.steam/steam # Flatpak
```

---

#### `sort <coluna> [asc|desc]`
Ordena a lista de jogos por uma coluna específica.

**Parâmetros:**
- `<coluna>` (obrigatório) - Coluna para ordenação:
  - `title` - Nome do jogo (padrão)
  - `playtime` - Tempo total de jogo
  - `achievements` - Conquistas desbloqueadas
  - `percentage` - Percentual de conclusão
  - `firstsession` - Data da primeira sessão (Como esse dado não é público na API, usa-se a data do primeiro achievement)
  - `lastsession` - Data da última sessão
  
- `[asc|desc]` (opcional) - Direção:
  - `asc` - Ascendente (A-Z, menor para maior) **[padrão]**
  - `desc` - Descendente (Z-A, maior para menor)

**Exemplos:**
```
sort title              # Ordena por título (A-Z)
sort playtime desc      # Ordena por tempo de jogo (maior para menor)
sort achievements       # Ordena por conquistas desbloqueadas
sort lastsession desc   # Jogos recentes primeiro
```

---

#### `exit`
Sai da aplicação.

**Uso:**
```
exit
```

---

## 📦 Dependências

- **Spectre.Console**
- **EntityFramework Core**
- **Microsoft.Extensions.Configuration**
- **Gameloop.Vdf**

## 🐛 Troubleshooting

### "Steam API Key not configured"
- Crie/atualize `appsettings.Secret.json` com sua API Key válida

### "Steam folder not found"
- O aplicativo tenta detectar automaticamente
- Se falhar, use o comando `folder` para indicar o path correto da pasta Steam no seu sistema.

### Nenhum jogo aparece após sync
- Verifique se o usuário Steam tem jogos com visibilidade pública
- Para API: jogos devem estar públicos no perfil
- Para local: só funciona com contas que já logaram no PC

### Erro ao compilar
- Verifique se possui .NET 10.0: `dotnet --version`
- Execute: `dotnet restore` para restaurar dependências

---

**Desenvolvido por Guglis @2025**
