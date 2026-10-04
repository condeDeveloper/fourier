namespace Conde.Fourier;

/// <summary>
/// A transformada de QUALQUER tamanho, pelo truque de Bluestein, de 1970.
///
/// A transformada rapida de Cooley e Tukey exige tamanho potencia de dois, e a
/// saida obvia e completar com zeros ate a proxima potencia. Isso funciona para
/// algumas coisas e NAO e a transformada que se queria: a transformada de um
/// sinal de 100 pontos completado para 128 tem 128 saidas, em frequencias que
/// nao sao as 100 originais, e comparar as duas lado a lado da numeros
/// diferentes em quase toda posicao.
///
/// O truque de Bluestein devolve a transformada verdadeira de n pontos, para n
/// qualquer, e a ideia e uma identidade de colegio: j*k = (j*j + k*k - (k-j)^2)/2.
/// Com ela, a soma da transformada vira uma CONVOLUCAO, e convolucao se faz com
/// transformadas de tamanho potencia de dois, que podem ser maiores a vontade.
///
/// O preco sao tres transformadas de tamanho pelo menos 2n-1 em vez de uma de n.
/// </summary>
public static class Bluestein
{
    /// <summary>A transformada direta de n pontos, para n qualquer.</summary>
    public static Complexo[] Transformar(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        return Rodar(entrada, sinal: -1);
    }

    /// <summary>A inversa de n pontos, para n qualquer.</summary>
    public static Complexo[] Inverter(IReadOnlyList<Complexo> entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        var n = entrada.Count;
        var saida = Rodar(entrada, sinal: 1);
        for (var i = 0; i < n; i++) saida[i] = saida[i] * (1.0 / n);
        return saida;
    }

    private static Complexo[] Rodar(IReadOnlyList<Complexo> entrada, int sinal)
    {
        var n = entrada.Count;
        if (n == 0) return [];
        if (n == 1) return [entrada[0]];

        // O angulo e pi*j*j/n, e j*j estoura rapido. Tirar o resto por 2n antes
        // nao e otimizacao: com n = 1000 e j = 999, j*j passa de um milhao, e o
        // cosseno de um angulo enorme perde quase todos os digitos, porque a
        // reducao interna do cosseno trabalha com um pi de precisao finita.
        var fase = new Complexo[n];
        for (var j = 0; j < n; j++)
        {
            var resto = (long)j * j % (2L * n);
            var angulo = sinal * Math.PI * resto / n;
            fase[j] = new Complexo(Math.Cos(angulo), Math.Sin(angulo));
        }

        var tamanho = Bits.ProximaPotencia(2 * n - 1);

        var a = new Complexo[tamanho];
        for (var j = 0; j < n; j++) a[j] = entrada[j] * fase[j];

        var b = new Complexo[tamanho];
        b[0] = fase[0].Conjugado;
        for (var j = 1; j < n; j++)
        {
            b[j] = fase[j].Conjugado;
            b[tamanho - j] = fase[j].Conjugado;
        }

        var produto = Rapida.Transformar(a);
        var outro = Rapida.Transformar(b);
        for (var i = 0; i < tamanho; i++) produto[i] *= outro[i];
        var convolucao = Rapida.Inverter(produto);

        var saida = new Complexo[n];
        for (var k = 0; k < n; k++) saida[k] = convolucao[k] * fase[k];
        return saida;
    }

    /// <summary>
    /// Quantas multiplicacoes complexas o truque gasta: tres transformadas do
    /// tamanho completado, mais a multiplicacao ponto a ponto e as duas fases.
    /// </summary>
    public static long Multiplicacoes(int n)
    {
        if (n <= 1) return 0;
        var tamanho = Bits.ProximaPotencia(2 * n - 1);
        return 3 * Rapida.Multiplicacoes(tamanho) + tamanho + 2L * n;
    }
}
