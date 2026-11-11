using System.Reflection;
using DetectorPhishing;
using DetectorPhishing.Domain;
using DetectorPhishing.Application;
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
    public static void EmpresaNaoIdentificadaSemOutrasInconsistenciasNaoGeraRelatorio()
    {
        var consulta = new ConsultaFake
        {
            EmpresasIdentificadas = Array.Empty<string>(),
        };
        var analista = new EmailAnalist(consulta);
        var conteudo = new ConteudoRelevante(
            "conteudo sem empresa reconhecida", "remetente@desconhecido.com", string.Empty,
            new[] { "http://desconhecido.com/pagina" });

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
