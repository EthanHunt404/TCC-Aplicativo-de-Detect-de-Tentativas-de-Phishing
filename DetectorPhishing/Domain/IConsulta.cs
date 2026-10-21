namespace DetectorPhishing.Domain;

/// <summary>
/// Interface entre a regra de negocio (EmailAnalist) e a implementacao
/// solida de acesso a dados (MongoDBInterface, em Infrastructure).
/// Declarada no Domain por inversao de dependencia (ADR 0005 / padrao
/// de codificacao, secao 3-D).
/// </summary>
public interface IConsulta
{
    string[] ConsultarDominiosEncurtamento(string hyperlink);
    string[] IdentificarEmpresa(string conteudo);
    string ConsultarTemplateEmpresa(string empresa);
}
