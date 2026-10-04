namespace Conde.Fourier;

/// <summary>
/// A INVERSAO DE BITS, que e a parte da transformada rapida que ninguem
/// explica.
///
/// A versao recursiva de Cooley e Tukey separa pares e impares a cada nivel. A
/// versao iterativa faz a mesma coisa de baixo para cima, e para isso a entrada
/// precisa ja estar na ordem em que a recursao teria deixado: o elemento que
/// acaba na posicao j e o da posicao com os BITS DE j AO CONTRARIO.
///
/// Isso nao e uma otimizacao esperta: e a consequencia de separar por par e
/// impar repetidamente, que e separar pelo bit menos significativo, depois pelo
/// seguinte, e assim por diante. No fim, a posicao de cada elemento e o indice
/// original lido de tras para frente.
/// </summary>
public static class Bits
{
    /// <summary>Se o numero e potencia de dois. Zero nao e.</summary>
    public static bool PotenciaDeDois(int n) => n > 0 && (n & (n - 1)) == 0;

    /// <summary>A menor potencia de dois maior ou igual a n.</summary>
    public static int ProximaPotencia(int n)
    {
        if (n <= 1) return 1;
        var saida = 1;
        while (saida < n) saida <<= 1;
        return saida;
    }

    /// <summary>Quantos bits tem o indice de um vetor de n posicoes.</summary>
    public static int Largura(int n)
    {
        if (!PotenciaDeDois(n)) throw new ArgumentException("nao e potencia de dois", nameof(n));
        var bits = 0;
        while (1 << bits < n) bits++;
        return bits;
    }

    /// <summary>O indice com os <paramref name="bits"/> bits de baixo ao contrario.</summary>
    public static int Inverter(int indice, int bits)
    {
        var saida = 0;
        for (var i = 0; i < bits; i++)
        {
            saida <<= 1;
            saida |= (indice >> i) & 1;
        }
        return saida;
    }

    /// <summary>
    /// Reordena o vetor no lugar, trocando cada posicao com a do indice
    /// invertido.
    ///
    /// A condicao "so troca quando i e menor" nao e economia: sem ela, cada par
    /// seria trocado DUAS vezes e o vetor voltaria ao que era. Os indices que
    /// sao o proprio reverso, como 0 e o ultimo, ficam parados nos dois casos.
    /// </summary>
    public static void Reordenar<T>(T[] vetor)
    {
        ArgumentNullException.ThrowIfNull(vetor);
        var bits = Largura(vetor.Length);

        for (var i = 0; i < vetor.Length; i++)
        {
            var j = Inverter(i, bits);
            if (i < j) (vetor[i], vetor[j]) = (vetor[j], vetor[i]);
        }
    }
}
