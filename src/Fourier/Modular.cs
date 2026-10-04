namespace Conde.Fourier;

/// <summary>
/// A transformada em ARITMETICA INTEIRA, sem um unico numero de ponto
/// flutuante.
///
/// A transformada rapida precisa de raizes da unidade, e e por isso que ela
/// costuma viver em complexos. Mas raiz da unidade e uma propriedade
/// ALGEBRICA, nao geometrica: basta um numero g com g^n = 1 e g^k diferente de
/// 1 para todo k menor. Num corpo de restos modulo um primo bem escolhido,
/// esses numeros existem.
///
/// O primo daqui e 998244353, que e 119 * 2^23 + 1. O 2^23 no meio nao e sorte:
/// ele garante raiz da unidade para todo tamanho potencia de dois ate 8 milhoes.
/// A raiz primitiva do corpo e 3.
///
/// O valor disso para o repositorio e ser um juiz EXATO. A transformada
/// complexa erra em ponto flutuante e a pergunta "errou quanto?" precisa de um
/// resultado certo para comparar. Esta aqui nao erra nada: o resultado e inteiro
/// e e o resultado.
/// </summary>
public static class Modular
{
    /// <summary>O primo 119 * 2^23 + 1, escolhido pelo 2^23.</summary>
    public const long Primo = 998244353;

    /// <summary>A raiz primitiva do corpo.</summary>
    public const long Raiz = 3;

    /// <summary>O maior tamanho potencia de dois que o primo suporta.</summary>
    public const int MaiorTamanho = 1 << 23;

    public static long Potencia(long baixo, long expoente, long modulo = Primo)
    {
        long saida = 1;
        baixo %= modulo;
        while (expoente > 0)
        {
            if ((expoente & 1) == 1) saida = saida * baixo % modulo;
            baixo = baixo * baixo % modulo;
            expoente >>= 1;
        }
        return saida;
    }

    /// <summary>
    /// O inverso de a, pelo pequeno teorema de Fermat: a^(p-2) mod p.
    ///
    /// Vale porque p e primo. Com modulo composto seria preciso o algoritmo de
    /// Euclides estendido, e nem sempre existiria inverso.
    /// </summary>
    public static long Inverso(long a) => Potencia(a, Primo - 2);

    /// <summary>
    /// A transformada rapida, com a MESMA estrutura da complexa: mesma inversao
    /// de bits, mesmas borboletas, so trocando a raiz da unidade.
    ///
    /// Esse paralelo e o assunto. A transformada rapida nao e sobre senos e
    /// cossenos: e sobre um grupo ciclico de ordem n. Os complexos sao um lugar
    /// onde esse grupo existe, e nao o unico.
    /// </summary>
    public static long[] Transformar(IReadOnlyList<long> entrada, bool inverso = false)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var n = entrada.Count;
        if (n == 0) return [];
        if (!Bits.PotenciaDeDois(n)) throw new ArgumentException("tamanho precisa ser potencia de dois", nameof(entrada));
        if (n > MaiorTamanho) throw new ArgumentException("tamanho maior do que o primo suporta", nameof(entrada));

        var vetor = entrada.Select(v => ((v % Primo) + Primo) % Primo).ToArray();
        Bits.Reordenar(vetor);

        for (var bloco = 2; bloco <= n; bloco <<= 1)
        {
            // A raiz de ordem 'bloco' sai da raiz primitiva elevada a
            // (p-1)/bloco. O 2^23 dentro do primo e o que faz essa divisao ser
            // exata para todo bloco potencia de dois.
            var passo = Potencia(Raiz, (Primo - 1) / bloco);
            if (inverso) passo = Inverso(passo);

            var metade = bloco / 2;
            for (var inicio = 0; inicio < n; inicio += bloco)
            {
                long raiz = 1;
                for (var j = 0; j < metade; j++)
                {
                    var a = vetor[inicio + j];
                    var b = vetor[inicio + j + metade] * raiz % Primo;
                    vetor[inicio + j] = (a + b) % Primo;
                    vetor[inicio + j + metade] = (a - b + Primo) % Primo;
                    raiz = raiz * passo % Primo;
                }
            }
        }

        if (!inverso) return vetor;

        var dividir = Inverso(n);
        for (var i = 0; i < n; i++) vetor[i] = vetor[i] * dividir % Primo;
        return vetor;
    }

    public static long[] Inverter(IReadOnlyList<long> entrada) => Transformar(entrada, inverso: true);
}
