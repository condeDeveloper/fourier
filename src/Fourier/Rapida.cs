namespace Conde.Fourier;

/// <summary>
/// A transformada rapida de Cooley e Tukey, de 1965, na forma iterativa.
///
/// A ideia cabe numa frase: a transformada de n pontos se escreve com duas de
/// n/2, uma sobre os indices pares e outra sobre os impares, mais n/2
/// multiplicacoes para juntar. Repetindo, o custo vai de n*n para (n/2)*log(n)
/// multiplicacoes complexas.
///
/// Gauss escreveu o mesmo algoritmo em 1805, para interpolar a orbita do
/// asteroide Pallas, e o texto so foi publicado depois da morte dele, em latim,
/// e ninguem percebeu. Cento e sessenta anos depois ele foi redescoberto e virou
/// um dos algoritmos mais usados do mundo.
/// </summary>
public static class Rapida
{
    /// <summary>
    /// A transformada direta. Exige tamanho potencia de dois; para o resto, veja
    /// <see cref="Bluestein"/>.
    /// </summary>
    public static Complexo[] Transformar(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        return Rodar([.. entrada], inverso: false);
    }

    /// <summary>
    /// A inversa, pelo truque do CONJUGADO: conjuga, transforma, conjuga de
    /// novo, divide por n.
    ///
    /// Escrever uma segunda rotina com o sinal trocado funcionaria e seria pior:
    /// duas copias quase iguais sao duas chances de divergirem, e o sinal do
    /// expoente e exatamente o tipo de detalhe que se corrige num lugar e se
    /// esquece no outro.
    /// </summary>
    public static Complexo[] Inverter(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var n = entrada.Count;
        var saida = Rodar([.. entrada.Select(c => c.Conjugado)], inverso: false);
        for (var i = 0; i < n; i++) saida[i] = saida[i].Conjugado * (1.0 / n);
        return saida;
    }

    private static Complexo[] Rodar(Complexo[] vetor, bool inverso)
    {
        var n = vetor.Length;
        if (n == 0) return vetor;
        if (!Bits.PotenciaDeDois(n))
            throw new ArgumentException("a transformada radical de dois exige tamanho potencia de dois", nameof(vetor));

        Bits.Reordenar(vetor);

        // De baixo para cima: junta pares de 1, depois de 2, de 4... Em cada
        // nivel, o passo vale metade do bloco, e a raiz anda uma fracao do
        // bloco por vez.
        for (var bloco = 2; bloco <= n; bloco <<= 1)
        {
            var metade = bloco / 2;
            for (var inicio = 0; inicio < n; inicio += bloco)
                for (var j = 0; j < metade; j++)
                {
                    var raiz = Complexo.Raiz(inverso ? j : -j, bloco);
                    var a = vetor[inicio + j];
                    var b = vetor[inicio + j + metade] * raiz;
                    vetor[inicio + j] = a + b;
                    vetor[inicio + j + metade] = a - b;
                }
        }

        return vetor;
    }

    /// <summary>
    /// Quantas multiplicacoes complexas a transformada rapida gasta: uma por
    /// borboleta, e sao (n/2)*log2(n) borboletas.
    /// </summary>
    public static long Multiplicacoes(int n) => n <= 1 ? 0 : (long)(n / 2) * Bits.Largura(n);
}
