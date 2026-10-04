namespace Conde.Fourier;

/// <summary>
/// A transformada pela DEFINICAO: uma soma de n termos para cada uma das n
/// saidas.
///
/// Ela e o JUIZ do repositorio. Nao tem truque, nao tem recursao, nao tem
/// embaralho de indice, e e lenta de proposito: cada linha e a formula copiada
/// do papel. Toda implementacao rapida daqui e conferida contra ela.
///
/// Ter o juiz lento importa porque as transformadas rapidas erram de um jeito
/// TRAICOEIRO. Um expoente com sinal trocado, uma ordem de bits invertida na
/// direcao errada ou um fator de escala fora do lugar produzem uma saida com o
/// mesmo tamanho, os mesmos modulos e a mesma cara de espectro. Nenhum teste de
/// "parece certo" pega isso.
/// </summary>
public static class Direta
{
    /// <summary>
    /// A transformada direta: X[k] = soma de x[j] * e^(-2*pi*i*j*k/n).
    ///
    /// Custa n*n multiplicacoes complexas, e esse numero e o ponto de partida de
    /// todas as comparacoes do repositorio.
    /// </summary>
    public static Complexo[] Transformar(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var n = entrada.Count;
        var saida = new Complexo[n];

        for (var k = 0; k < n; k++)
        {
            var soma = Complexo.Zero;
            for (var j = 0; j < n; j++)
                soma += entrada[j] * Complexo.Raiz(-j * k, n);
            saida[k] = soma;
        }

        return saida;
    }

    /// <summary>
    /// A inversa: o mesmo com o sinal do expoente trocado, dividido por n.
    ///
    /// A divisao por n podia estar na ida, ou repartida em raiz de n nos dois
    /// lados, e as tres convencoes existem na literatura. O que nao pode e
    /// misturar: com a escala em lugar nenhum a ida e volta devolve o sinal
    /// multiplicado por n, e com ela nos dois lugares devolve dividido por n.
    /// </summary>
    public static Complexo[] Inverter(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var n = entrada.Count;
        var saida = new Complexo[n];

        for (var j = 0; j < n; j++)
        {
            var soma = Complexo.Zero;
            for (var k = 0; k < n; k++)
                soma += entrada[k] * Complexo.Raiz(j * k, n);
            saida[j] = soma * (1.0 / n);
        }

        return saida;
    }

    /// <summary>Quantas multiplicacoes complexas a definicao gasta.</summary>
    public static long Multiplicacoes(int n) => (long)n * n;
}
