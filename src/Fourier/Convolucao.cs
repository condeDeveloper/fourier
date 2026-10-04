namespace Conde.Fourier;

/// <summary>
/// A CONVOLUCAO, que e para que a transformada serve na pratica.
///
/// Convolver dois vetores e multiplicar dois polinomios, e multiplicar dois
/// polinomios e multiplicar dois numeros grandes. O teorema da convolucao diz
/// que no dominio da frequencia isso vira multiplicacao ponto a ponto, e e so
/// por causa dele que a transformada rapida importa fora do processamento de
/// sinal.
///
/// Aqui ela e feita de tres jeitos: pela definicao, pela transformada complexa e
/// pela transformada inteira. Os tres tem que concordar, e o lugar onde deixam
/// de concordar e a medida mais util do repositorio.
/// </summary>
public static class Convolucao
{
    /// <summary>
    /// Pela definicao: cada par de posicoes contribui para a soma dos indices.
    /// Custa n*m multiplicacoes e e o juiz.
    /// </summary>
    public static long[] Definicao(IReadOnlyList<long> a, IReadOnlyList<long> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count == 0 || b.Count == 0) return [];

        var saida = new long[a.Count + b.Count - 1];
        for (var i = 0; i < a.Count; i++)
            for (var j = 0; j < b.Count; j++)
                saida[i + j] += a[i] * b[j];
        return saida;
    }

    /// <summary>
    /// Pela transformada COMPLEXA: transforma os dois, multiplica ponto a
    /// ponto, inverte, e arredonda.
    ///
    /// O arredondamento e a parte perigosa e a razao de existir a medida de
    /// precisao. O resultado certo e inteiro, o calculado nao e, e enquanto o
    /// erro ficar abaixo de meio, arredondar conserta tudo. Quando passa de
    /// meio, o arredondamento conserta para o numero ERRADO, sem aviso nenhum.
    /// </summary>
    public static long[] PorComplexo(IReadOnlyList<long> a, IReadOnlyList<long> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count == 0 || b.Count == 0) return [];

        var tamanho = Bits.ProximaPotencia(a.Count + b.Count - 1);

        var x = new Complexo[tamanho];
        var y = new Complexo[tamanho];
        for (var i = 0; i < a.Count; i++) x[i] = new Complexo(a[i], 0);
        for (var i = 0; i < b.Count; i++) y[i] = new Complexo(b[i], 0);

        var fx = Rapida.Transformar(x);
        var fy = Rapida.Transformar(y);
        for (var i = 0; i < tamanho; i++) fx[i] *= fy[i];
        var volta = Rapida.Inverter(fx);

        var saida = new long[a.Count + b.Count - 1];
        for (var i = 0; i < saida.Length; i++) saida[i] = (long)Math.Round(volta[i].Real);
        return saida;
    }

    /// <summary>
    /// Pela transformada INTEIRA: o mesmo caminho, sem ponto flutuante nenhum.
    ///
    /// O resultado e exato modulo o primo. Enquanto o maior coeficiente da
    /// resposta couber abaixo do primo, exato quer dizer exato mesmo.
    /// </summary>
    public static long[] PorInteiro(IReadOnlyList<long> a, IReadOnlyList<long> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count == 0 || b.Count == 0) return [];

        var tamanho = Bits.ProximaPotencia(a.Count + b.Count - 1);

        var x = new long[tamanho];
        var y = new long[tamanho];
        for (var i = 0; i < a.Count; i++) x[i] = a[i];
        for (var i = 0; i < b.Count; i++) y[i] = b[i];

        var fx = Modular.Transformar(x);
        var fy = Modular.Transformar(y);
        for (var i = 0; i < tamanho; i++) fx[i] = fx[i] * fy[i] % Modular.Primo;
        var volta = Modular.Inverter(fx);

        return [.. volta.Take(a.Count + b.Count - 1)];
    }

    /// <summary>
    /// A convolucao CIRCULAR, que e a que a transformada faz de verdade.
    ///
    /// Multiplicar as transformadas ponto a ponto NAO da a convolucao comum: da
    /// a circular, em que o que passa do fim volta para o comeco. As duas so
    /// coincidem quando ha zeros suficientes no fim, e e por isso que os metodos
    /// de cima completam ate o proximo tamanho e nao usam o tamanho justo.
    ///
    /// Esta funcao existe para o teste exibir a diferenca em vez de a explicar.
    /// </summary>
    public static long[] Circular(IReadOnlyList<long> a, IReadOnlyList<long> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count != b.Count) throw new ArgumentException("a circular pede tamanhos iguais", nameof(b));

        var n = a.Count;
        var saida = new long[n];
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                saida[(i + j) % n] += a[i] * b[j];
        return saida;
    }

    /// <summary>Quantas multiplicacoes cada caminho gasta, para a tabela.</summary>
    public static (long Definicao, long Complexa, long Inteira) Custo(int n, int m)
    {
        var tamanho = Bits.ProximaPotencia(n + m - 1);
        return ((long)n * m,
                3 * Rapida.Multiplicacoes(tamanho) + tamanho,
                3 * Rapida.Multiplicacoes(tamanho) + tamanho);
    }
}
