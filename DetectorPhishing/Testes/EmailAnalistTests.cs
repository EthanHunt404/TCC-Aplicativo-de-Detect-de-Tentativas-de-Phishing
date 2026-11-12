using System.Reflection;
using System.Text;
using System.Threading.Channels;
using DetectorPhishing;
using DetectorPhishing.Domain;
using DetectorPhishing.Application;
using DetectorPhishing.Infrastructure;
using MimeKit;
using Xunit;

namespace DetectorPhishing.Testes;

public static class EmailAnalistTests
{
    [Fact]
    public static void EmailSemHyperlinkEhIgnorado()
    {
        IConsulta consulta = new ConsultaFake();
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Sem links aqui", "remetente@empresa.com", "Empresa", Array.Empty<string>());

        var relatorio = analista.Analisar(conteudo);

        Assert.Null(relatorio);
    }

    [Fact]
    public static void HyperlinkEncurtadoGeraInconsistencia()
    {
        var consulta = new ConsultaFake
        {
            DominiosEncurtamento = new[] { "bit.ly" },
            EmpresasIdentificadas = new[] { "Empresa" },
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "conteudo qualquer", "remetente@empresa.com", "Empresa",
            new[] { "http://bit.ly/xyz" });

        var relatorio = analista.Analisar(conteudo);

        Assert.NotNull(relatorio);
        Assert.Contains(relatorio!.Inconsistencias, i => i.Tipo == "LinkEncurtado");
    }

    [Fact]
    public static void EnderecoRemetenteForaDoDominioOficialGeraInconsistencia()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = new[] { "Banco Exemplo" },
            DominioOficial = "banco-exemplo.com.br",
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Banco Exemplo: confirme seus dados", "atendimento@banco-exemplo-fraude.com", "Banco Exemplo",
            new[] { "http://banco-exemplo.com.br/pagina" });

        var relatorio = analista.Analisar(conteudo);

        Assert.NotNull(relatorio);
        Assert.Contains(relatorio!.Inconsistencias, i => i.Tipo == "EnderecoRemetenteDivergente");
    }

    [Fact]
    public static void HyperlinkForaDoDominioOficialGeraInconsistencia()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = new[] { "Banco Exemplo" },
            DominioOficial = "banco-exemplo.com.br",
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Banco Exemplo: confirme seus dados", "atendimento@banco-exemplo.com.br", "Banco Exemplo",
            new[] { "http://banco-exemplo-fraude.com/login" });

        var relatorio = analista.Analisar(conteudo);

        Assert.NotNull(relatorio);
        Assert.Contains(relatorio!.Inconsistencias, i => i.Tipo == "HyperlinkDivergente");
    }

    [Fact]
    public static void SubdominioDoDominioOficialNaoGeraInconsistencia()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = new[] { "Banco Exemplo" },
            DominioOficial = "banco-exemplo.com.br",
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Banco Exemplo: sua fatura chegou", "faturas@contas.banco-exemplo.com.br", "Banco Exemplo",
            new[] { "http://contas.banco-exemplo.com.br/fatura" });

        var relatorio = analista.Analisar(conteudo);

        Assert.Null(relatorio);
    }

    [Fact]
    public static void EmailLegitimoNaoGeraRelatorio()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = new[] { "Banco Exemplo" },
            DominioOficial = "banco-exemplo.com.br",
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Banco Exemplo: sua fatura chegou", "faturas@banco-exemplo.com.br", "Banco Exemplo",
            new[] { "http://banco-exemplo.com.br/fatura" });

        var relatorio = analista.Analisar(conteudo);

        Assert.Null(relatorio);
    }

    [Fact]
    public static void EmpresaNaoIdentificadaEhIgnoradaMesmoComLinkEncurtado()
    {
        var consulta = new ConsultaFake
        {
            DominiosEncurtamento = new[] { "bit.ly" },
            EmpresasIdentificadas = new[] { "nenhum" },
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "conteudo sem empresa reconhecida", "remetente@desconhecido.com", string.Empty,
            new[] { "http://bit.ly/xyz" });

        var relatorio = analista.Analisar(conteudo);

        Assert.Null(relatorio);
    }

    [Fact]
    public static void EmpresaSemTemplateRegistradoNaoGeraInconsistenciaDeDominio()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = new[] { "Empresa Sem Template" },
            DominioOficial = string.Empty,
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "Empresa Sem Template avisa algo", "contato@empresa-sem-template.com", "Empresa Sem Template",
            new[] { "http://empresa-sem-template.com/pagina" });

        var relatorio = analista.Analisar(conteudo);

        Assert.Null(relatorio);
    }

    [Fact]
    public static void ConteudoNuloEhIgnorado()
    {
        var analista = new EmailAnalist(new ConsultaFake());

        Assert.Null(analista.Analisar(null!));
    }

    // ---- Ponta a ponta com dados dummy (ConsultaDummy) ----

    [Fact]
    public static void DummyEmailDePhishingGeraRelatorioComTodasAsInconsistencias()
    {
        var analista = new EmailAnalist(new ConsultaDummy());
        var conteudo = new ConteudoRelevante(
            "Prezado cliente, seu Banco Exemplo precisa de confirmacao urgente. Acesse: http://bit.ly/abc123",
            "atendimento@banco-exemplo-seguro.com", "Banco Exemplo",
            new[] { "http://bit.ly/abc123", "http://banco-exemplo-seguro.com/login" });

        var relatorio = analista.Analisar(conteudo);

        Assert.NotNull(relatorio);
        Assert.Contains(relatorio!.Inconsistencias, i => i.Tipo == "LinkEncurtado");
        Assert.Contains(relatorio.Inconsistencias, i => i.Tipo == "EnderecoRemetenteDivergente");
        Assert.Contains(relatorio.Inconsistencias, i => i.Tipo == "HyperlinkDivergente");
    }

    [Fact]
    public static void DummyEmailLegitimoNaoGeraRelatorio()
    {
        var analista = new EmailAnalist(new ConsultaDummy());
        var conteudo = new ConteudoRelevante(
            "Banco Exemplo: sua fatura esta disponivel.",
            "faturas@banco-exemplo.com.br", "Banco Exemplo",
            new[] { "http://banco-exemplo.com.br/faturas" });

        Assert.Null(analista.Analisar(conteudo));
    }

    [Fact]
    public static void DummyEmailSemHyperlinkEhIgnorado()
    {
        var analista = new EmailAnalist(new ConsultaDummy());
        var conteudo = new ConteudoRelevante(
            "Reuniao de equipe amanha as 10h.",
            "colega@empresa-interna.com", string.Empty, Array.Empty<string>());

        Assert.Null(analista.Analisar(conteudo));
    }

    [Fact]
    public static void AmostrasEml_AbremNoMimeKit()
    {
        var diretorioAmostras = Path.Combine(Directory.GetCurrentDirectory(), "base", "amostras");
        var arquivos = Directory.GetFiles(diretorioAmostras, "S-*.eml")
            .OrderBy(caminho => caminho, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(15, arquivos.Length);

        foreach (var arquivo in arquivos)
        {
            var mensagem = MimeMessage.Load(arquivo);

            Assert.NotNull(mensagem);
            Assert.NotEmpty(mensagem.From);
        }
    }

    [Fact]
    public static void ExtratorDeConteudo_RespeitaRegrasDasAmostras()
    {
        var diretorioAmostras = Path.Combine(Directory.GetCurrentDirectory(), "base", "amostras");

        for (var numero = 1; numero <= 15; numero++)
        {
            var arquivo = Path.Combine(diretorioAmostras, $"S-{numero:D2}.eml");
            var mensagem = MimeMessage.Load(arquivo);
            var conteudo = ExtratorDeConteudo.Extrair(mensagem);

            if (numero is 5 or 6 or 15)
            {
                Assert.Null(conteudo);
                continue;
            }

            Assert.NotNull(conteudo);
            if (numero == 14)
                continue;

            Assert.Collection(conteudo!.Hyperlinks, hyperlink => Assert.NotEmpty(hyperlink));
            Assert.False(string.IsNullOrWhiteSpace(conteudo.EnderecoRemetente));
        }

        var amostraDeVinteLinks = ExtratorDeConteudo.Extrair(
            MimeMessage.Load(Path.Combine(diretorioAmostras, "S-14.eml")));
        Assert.NotNull(amostraDeVinteLinks);
        Assert.Equal(
            Enumerable.Range(1, 19)
                .Select(indice => $"https://www.lojaexemplo.com.br/oferta/{indice:D2}")
                .Append("https://lojaexemplo-promo.com/x"),
            amostraDeVinteLinks!.Hyperlinks);

        var amostraSomenteTexto = ExtratorDeConteudo.Extrair(
            MimeMessage.Load(Path.Combine(diretorioAmostras, "S-12.eml")));
        Assert.NotNull(amostraSomenteTexto);
        Assert.Contains("https://www.bancoexemplo.com.br/atendimento", amostraSomenteTexto!.Hyperlinks);
        Assert.Contains("BancoExemplo: acesse https://www.bancoexemplo.com.br/atendimento para consultar o atendimento.", amostraSomenteTexto.ConteudoBruto);
    }

    [Fact]
    public static void ExtratorDeConteudo_IgnoraLinksInvalidosEDeduplicaNaOrdem()
    {
        const string url = "https://empresa.exemplo/caminho";
        var mensagem = new MimeMessage
        {
            From = { new MailboxAddress("Remetente", "remetente@empresa.exemplo") },
            Body = new Multipart("alternative")
            {
                new TextPart("plain") { Text = $"Acesse {url} e depois {url}." },
                new TextPart("html")
                {
                    Text = $"<a href=\"mailto:contato@empresa.exemplo\">Contato</a><a href=\"/relativo\">Relativo</a><a href=\"{url}\">Acesso</a>"
                }
            }
        };

        var conteudo = ExtratorDeConteudo.Extrair(mensagem);

        Assert.NotNull(conteudo);
        Assert.Equal(new[] { url }, conteudo!.Hyperlinks);
    }

    [Fact]
    public static async Task GravadorRelatorioTxt_GravaFormatoComUtf8SemBom()
    {
        var diretorioTemporario = Path.Combine(Path.GetTempPath(), "PhishingDetectorTest_" + Guid.NewGuid().ToString("N"));
        var dataGeracao = new DateTime(2026, 11, 12, 19, 0, 0, 123, DateTimeKind.Local);
        var relatorio = new RelatorioInconsistencias(
            "Corpo da mensagem de teste.",
            new[] { new Inconsistencia("HyperlinkDivergente", "Link divergente de teste.") },
            dataGeracao);

        try
        {
            var gravador = new GravadorRelatorioTxt(diretorioTemporario);
            var caminho = await gravador.GravarAsync(relatorio);
            var bytes = File.ReadAllBytes(caminho);
            var texto = File.ReadAllText(caminho, Encoding.UTF8);
            var nomeEsperado = "relatorio_20261112_190000_123.txt";

            Assert.Equal(nomeEsperado, Path.GetFileName(caminho));
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Contains("RELATÓRIO DE INCONSISTÊNCIAS", texto);
            Assert.Contains("Data de geração: 2026-11-12T19:00:00.1230000-03:00", texto);
            Assert.Contains("Total de inconsistências: 1", texto);
            Assert.Contains("1. [HyperlinkDivergente] Link divergente de teste.", texto);
            Assert.Contains("--- CONTEÚDO ANALISADO ---", texto);
            Assert.EndsWith("Corpo da mensagem de teste.", texto);
        }
        finally
        {
            if (Directory.Exists(diretorioTemporario))
                Directory.Delete(diretorioTemporario, recursive: true);
        }
    }

    [Fact]
    public static async Task ServicoDeAnalise_GravaENotificaSomenteRelatoriosSuspeitos()
    {
        var canal = Channel.CreateUnbounded<ConteudoRelevante>();
        var gravador = new GravadorFalso();
        var notificador = new NotificadorFalso();
        var analista = new EmailAnalist(new ConsultaFake
        {
            DominiosEncurtamento = new[] { "bit.ly" },
            EmpresasIdentificadas = new[] { "Empresa" },
        });
        var servico = new ServicoDeAnalise(canal.Reader, analista, gravador, notificador);

        await canal.Writer.WriteAsync(new ConteudoRelevante(
            "Acesse https://bit.ly/abc", "remetente@exemplo.invalid", string.Empty,
            new[] { "https://bit.ly/abc" }));
        await canal.Writer.WriteAsync(new ConteudoRelevante(
            "Mensagem sem links", "remetente@exemplo.invalid", string.Empty,
            Array.Empty<string>()));
        canal.Writer.TryComplete();

        await servico.ExecutarAsync(CancellationToken.None);

        Assert.Single(gravador.Relatorios);
        Assert.Single(notificador.Caminhos);
        Assert.Equal("relatorio-falso.txt", notificador.Caminhos[0]);
    }

    [Fact]
    public static async Task ServicoDeAnalise_ContinuaAposFalhaAoGravarUmRelatorio()
    {
        var canal = Channel.CreateUnbounded<ConteudoRelevante>();
        var gravador = new GravadorFalso(falharPrimeiraGravacao: true);
        var notificador = new NotificadorFalso();
        var analista = new EmailAnalist(new ConsultaFake
        {
            DominiosEncurtamento = new[] { "bit.ly" },
            EmpresasIdentificadas = new[] { "Empresa" },
        });
        var servico = new ServicoDeAnalise(canal.Reader, analista, gravador, notificador);

        for (var indice = 0; indice < 2; indice++)
        {
            await canal.Writer.WriteAsync(new ConteudoRelevante(
                $"Mensagem suspeita {indice}: https://bit.ly/{indice}",
                "remetente@exemplo.invalid",
                string.Empty,
                new[] { $"https://bit.ly/{indice}" }));
        }

        canal.Writer.TryComplete();
        await servico.ExecutarAsync(CancellationToken.None);

        Assert.Equal(2, gravador.Tentativas);
        Assert.Single(gravador.Relatorios);
        Assert.Single(notificador.Caminhos);
    }

    [Fact]
    public static void ConfiguracaoAplicacao_CarregaMongoLocalDaConfiguracaoVersionada()
    {
        var configuracao = ConfiguracaoAplicacao.Carregar();

        Assert.Equal("mongodb://localhost:27017", configuracao.MongoUri);
        Assert.Equal("phishing_poc", configuracao.MongoDatabase);
    }

    [Fact]
    public static void Integracao_MongoDBInterfaceConsultaBaseDeValidacao()
    {
        var consulta = new MongoDBInterface(ConfiguracaoAplicacao.Carregar());

        Assert.Equal("BancoExemplo", Assert.Single(consulta.IdentificarEmpresa("Aviso para BancoExemplo")));
        Assert.Equal("Banco Nacional Exemplo", Assert.Single(consulta.IdentificarEmpresa("Aviso para Banco Nacional Exemplo")));
        Assert.Equal("nenhum", Assert.Single(consulta.IdentificarEmpresa("Empresa completamente desconhecida")));
        Assert.Equal("bancoexemplo.com.br", consulta.ConsultarTemplateEmpresa("BancoExemplo"));
        Assert.Equal("bit.ly/*", Assert.Single(consulta.ConsultarDominiosEncurtamento("https://bit.ly/3AbCdEf")));
    }

    [Fact]
    public static void Dominio_NaoReferencia_OutrasCamadas()
    {
        var tiposReferenciados = ObterTiposReferenciadosDaCamada("DetectorPhishing.Domain");

        Assert.DoesNotContain(tiposReferenciados, tipo =>
            PertenceACamada(tipo, "Application")
            || PertenceACamada(tipo, "Infrastructure")
            || PertenceACamada(tipo, "Presentation"));
    }

    [Fact]
    public static void Application_NaoReferencia_InfrastructureNemPresentation()
    {
        var tiposReferenciados = ObterTiposReferenciadosDaCamada("DetectorPhishing.Application");

        Assert.DoesNotContain(tiposReferenciados, tipo =>
            PertenceACamada(tipo, "Infrastructure")
            || PertenceACamada(tipo, "Presentation"));
    }

    private sealed class GravadorFalso(bool falharPrimeiraGravacao = false) : IGravadorRelatorio
    {
        public List<RelatorioInconsistencias> Relatorios { get; } = new();
        public int Tentativas { get; private set; }

        public Task<string> GravarAsync(RelatorioInconsistencias relatorio)
        {
            Tentativas++;
            if (falharPrimeiraGravacao && Tentativas == 1)
                throw new IOException("Falha de gravação simulada.");

            Relatorios.Add(relatorio);
            return Task.FromResult("relatorio-falso.txt");
        }
    }

    private sealed class NotificadorFalso : INotificador
    {
        public List<string> Caminhos { get; } = new();

        public void Notificar(string caminhoRelatorio) => Caminhos.Add(caminhoRelatorio);
    }

    private static IEnumerable<Type> ObterTiposReferenciadosDaCamada(string nomeNamespace)
    {
        var tiposDaCamada = typeof(EmailAnalist).Assembly.GetTypes()
            .Where(tipo => tipo.Namespace == nomeNamespace);

        return tiposDaCamada.SelectMany(ObterTiposReferenciados);
    }

    private static IEnumerable<Type> ObterTiposReferenciados(Type tipo)
    {
        if (tipo.BaseType != null)
        {
            foreach (var tipoBase in ExpandirTipo(tipo.BaseType))
                yield return tipoBase;
        }

        foreach (var interfaceTipo in tipo.GetInterfaces())
        {
            foreach (var tipoInterface in ExpandirTipo(interfaceTipo))
                yield return tipoInterface;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        foreach (var campo in tipo.GetFields(flags))
        {
            foreach (var tipoCampo in ExpandirTipo(campo.FieldType))
                yield return tipoCampo;
        }

        foreach (var propriedade in tipo.GetProperties(flags))
        {
            foreach (var tipoPropriedade in ExpandirTipo(propriedade.PropertyType))
                yield return tipoPropriedade;

            foreach (var parametro in propriedade.GetIndexParameters())
            {
                foreach (var tipoParametro in ExpandirTipo(parametro.ParameterType))
                    yield return tipoParametro;
            }
        }

        foreach (var metodo in tipo.GetMethods(flags))
        {
            foreach (var tipoRetorno in ExpandirTipo(metodo.ReturnType))
                yield return tipoRetorno;

            foreach (var parametro in metodo.GetParameters())
            {
                foreach (var tipoParametro in ExpandirTipo(parametro.ParameterType))
                    yield return tipoParametro;
            }
        }

        foreach (var construtor in tipo.GetConstructors(flags))
        {
            foreach (var parametro in construtor.GetParameters())
            {
                foreach (var tipoParametro in ExpandirTipo(parametro.ParameterType))
                    yield return tipoParametro;
            }
        }
    }

    private static IEnumerable<Type> ExpandirTipo(Type tipo)
    {
        yield return tipo;

        if (tipo.HasElementType && tipo.GetElementType() is { } tipoElemento)
        {
            foreach (var tipoExpandido in ExpandirTipo(tipoElemento))
                yield return tipoExpandido;
        }

        if (tipo.IsGenericType)
        {
            foreach (var argumento in tipo.GetGenericArguments())
            {
                foreach (var tipoExpandido in ExpandirTipo(argumento))
                    yield return tipoExpandido;
            }
        }
    }

    private static bool PertenceACamada(Type tipo, string camada)
    {
        var nomeNamespace = tipo.Namespace;
        var prefixo = $"DetectorPhishing.{camada}";

        return nomeNamespace == prefixo
            || nomeNamespace?.StartsWith(prefixo + ".", StringComparison.Ordinal) == true;
    }
}
