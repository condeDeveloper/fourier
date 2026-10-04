namespace Conde.Fourier;

/// <summary>
/// Os sinais de teste, e o sorteio com semente que os gera.
///
/// O gerador e escrito por extenso, e nao o Random do .NET, que nao promete a
/// mesma sequencia entre versoes nem entre sistemas. A CI roda em tres, e um
/// numero de erro que mudasse de sistema nao seria uma medida.
/// </summary>
public static class Sinais
{
    /// <summary>Um congruencial linear, com os mesmos numeros em todo lugar.</summary>
    public sealed class Aleatorio
    {
        private ulong estado;

        public Aleatorio(ulong semente) => estado = semente * 2 + 1;

        public ulong Proximo()
        {
            estado = unchecked(estado * 6364136223846793005UL + 1442695040888963407UL);
            return estado >> 17;
        }

        public long Entre(long baixo, long alto) => baixo + (long)(Proximo() % (ulong)(alto - baixo + 1));

        /// <summary>Um real entre -1 e 1.</summary>
        public double Real() => (Entre(0, 2_000_000) - 1_000_000) / 1_000_000.0;
    }

    /// <summary>
    /// O IMPULSO: um unico 1 na posicao zero.
    ///
    /// A transformada dele e CONSTANTE, igual a um em toda frequencia, e esse e
    /// o melhor primeiro teste que existe: ele pega sinal trocado, escala errada
    /// e ordem de bits invertida de uma vez, e da para conferir de cabeca.
    /// </summary>
    public static Complexo[] Impulso(int n)
    {
        var saida = new Complexo[n];
        if (n > 0) saida[0] = Complexo.Um;
        return saida;
    }

    /// <summary>
    /// A CONSTANTE: todos iguais a um.
    ///
    /// E o inverso do impulso: a transformada e n na frequencia zero e zero no
    /// resto. Os dois juntos conferem os dois extremos da escala.
    /// </summary>
    public static Complexo[] Constante(int n) => [.. Enumerable.Repeat(Complexo.Um, n)];

    /// <summary>
    /// Um cosseno que cabe exatamente <paramref name="ciclos"/> vezes na janela.
    ///
    /// Com ciclos inteiro, a transformada tem exatamente DUAS posicoes
    /// diferentes de zero, em k e em n-k, cada uma valendo n/2. E o unico caso
    /// em que o espectro e limpo.
    /// </summary>
    public static Complexo[] Cosseno(int n, double ciclos)
    {
        var saida = new Complexo[n];
        for (var j = 0; j < n; j++) saida[j] = new Complexo(Math.Cos(2 * Math.PI * ciclos * j / n), 0);
        return saida;
    }

    /// <summary>Uma onda quadrada com meio periodo de <paramref name="largura"/>.</summary>
    public static Complexo[] Quadrada(int n, int largura)
    {
        var saida = new Complexo[n];
        for (var j = 0; j < n; j++) saida[j] = new Complexo(j / largura % 2 == 0 ? 1 : -1, 0);
        return saida;
    }

    /// <summary>Um sinal sorteado, reproduzivel pela semente.</summary>
    public static Complexo[] Sorteado(int n, ulong semente)
    {
        var sorte = new Aleatorio(semente);
        var saida = new Complexo[n];
        for (var j = 0; j < n; j++) saida[j] = new Complexo(sorte.Real(), sorte.Real());
        return saida;
    }

    /// <summary>Inteiros sorteados ate um teto, para as convolucoes.</summary>
    public static long[] Inteiros(int n, long teto, ulong semente)
    {
        var sorte = new Aleatorio(semente);
        var saida = new long[n];
        for (var j = 0; j < n; j++) saida[j] = sorte.Entre(0, teto);
        return saida;
    }

    /// <summary>
    /// A ENERGIA do sinal, que e a soma dos quadrados dos modulos.
    ///
    /// Ela serve para o teorema de Parseval: a energia do sinal e a energia da
    /// transformada dividida por n. E uma conferencia independente da ida e
    /// volta, porque ela olha so a saida, sem desfazer nada.
    /// </summary>
    public static double Energia(IReadOnlyList<Complexo> sinal)
    {
        ArgumentNullException.ThrowIfNull(sinal);
        double soma = 0;
        foreach (var c in sinal) soma += c.Real * c.Real + c.Imaginario * c.Imaginario;
        return soma;
    }
}
