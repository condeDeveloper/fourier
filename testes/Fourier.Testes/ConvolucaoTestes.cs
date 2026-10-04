using Conde.Fourier;
using Xunit;

namespace Conde.Fourier.Testes;

public class ConvolucaoTestes
{
    /// <summary>
    /// Convolver e multiplicar polinomio. (1 + 2x)(3 + 4x) = 3 + 10x + 8x^2, e
    /// da para conferir de cabeca.
    /// </summary>
    [Fact]
    public void AConvolucaoEhMultiplicacaoDePolinomio()
    {
        Assert.Equal(new long[] { 3, 10, 8 }, Convolucao.Definicao([1, 2], [3, 4]));
    }

    [Fact]
    public void OsTresCaminhosConcordam()
    {
        foreach (var n in new[] { 1, 2, 3, 7, 16, 33, 64 })
        {
            var a = Sinais.Inteiros(n, 1000, (ulong)n);
            var b = Sinais.Inteiros(n, 1000, (ulong)(n + 100));

            var certo = Convolucao.Definicao(a, b);
            Assert.Equal(certo, Convolucao.PorComplexo(a, b));
            Assert.Equal(certo, Convolucao.PorInteiro(a, b));
        }
    }

    [Fact]
    public void TamanhosDiferentesTambemFuncionam()
    {
        var a = Sinais.Inteiros(5, 100, 1);
        var b = Sinais.Inteiros(12, 100, 2);
        Assert.Equal(Convolucao.Definicao(a, b), Convolucao.PorComplexo(a, b));
        Assert.Equal(Convolucao.Definicao(a, b), Convolucao.PorInteiro(a, b));
    }

    [Fact]
    public void ConvolverComImpulsoNaoMudaNada()
    {
        var a = Sinais.Inteiros(8, 50, 3);
        var resultado = Convolucao.PorInteiro(a, [1]);
        Assert.Equal(a, resultado);
    }

    /// <summary>
    /// Multiplicar as transformadas ponto a ponto da a convolucao CIRCULAR, em
    /// que o que passa do fim volta para o comeco. As duas so coincidem com
    /// zeros suficientes no fim, e e por isso que os metodos completam o
    /// tamanho.
    /// </summary>
    [Fact]
    public void ACircularNaoEhAComum()
    {
        long[] a = [1, 2, 3, 4];
        long[] b = [5, 6, 7, 8];

        var comum = Convolucao.Definicao(a, b);
        var circular = Convolucao.Circular(a, b);

        Assert.Equal(7, comum.Length);
        Assert.Equal(4, circular.Length);
        Assert.NotEqual(comum[0], circular[0]);

        // A circular e a comum dobrada: o que passa do tamanho volta.
        for (var i = 0; i < 4; i++)
        {
            long soma = 0;
            for (var j = i; j < comum.Length; j += 4) soma += comum[j];
            Assert.Equal(soma, circular[i]);
        }
    }

    [Fact]
    public void ComZerosSuficientesAsDuasCoincidem()
    {
        long[] a = [1, 2, 3, 4, 0, 0, 0, 0];
        long[] b = [5, 6, 7, 8, 0, 0, 0, 0];

        var comum = Convolucao.Definicao([1, 2, 3, 4], [5, 6, 7, 8]);
        var circular = Convolucao.Circular(a, b);

        for (var i = 0; i < comum.Length; i++) Assert.Equal(comum[i], circular[i]);
    }

    [Fact]
    public void VetorVazioDaResultadoVazio()
    {
        Assert.Empty(Convolucao.Definicao([], [1, 2]));
        Assert.Empty(Convolucao.PorComplexo([1, 2], []));
        Assert.Empty(Convolucao.PorInteiro([], []));
    }

    /// <summary>
    /// A transformada inteira e EXATA enquanto o resultado cabe abaixo do primo,
    /// e vira resto depois disso. Os dois lados do limite num teste.
    /// </summary>
    [Fact]
    public void AInteiraEhExataAteOPrimoEViraRestoDepois()
    {
        var pequenos = Sinais.Inteiros(64, 1 << 8, 11);
        var outros = Sinais.Inteiros(64, 1 << 8, 22);
        Assert.Equal(Convolucao.Definicao(pequenos, outros), Convolucao.PorInteiro(pequenos, outros));

        var grandes = Sinais.Inteiros(64, 1 << 16, 11);
        var maiores = Sinais.Inteiros(64, 1 << 16, 22);
        var certo = Convolucao.Definicao(grandes, maiores);
        var pelaInteira = Convolucao.PorInteiro(grandes, maiores);

        Assert.True(certo.Max() > Modular.Primo);
        Assert.NotEqual(certo, pelaInteira);
        for (var i = 0; i < certo.Length; i++)
            Assert.Equal(certo[i] % Modular.Primo, pelaInteira[i]);
    }

    /// <summary>
    /// E a complexa aguenta MAIS que a inteira neste caso, o que e o contrario
    /// do que o nome de cada uma sugere.
    /// </summary>
    [Fact]
    public void AComplexaAguentaMaisQueAInteiraNesteCaso()
    {
        var a = Sinais.Inteiros(256, 1 << 12, 11);
        var b = Sinais.Inteiros(256, 1 << 12, 22);
        var certo = Convolucao.Definicao(a, b);

        Assert.Equal(certo, Convolucao.PorComplexo(a, b));
        Assert.NotEqual(certo, Convolucao.PorInteiro(a, b));
    }

    [Fact]
    public void ATransformadaInteiraTemIdaEVolta()
    {
        var vetor = Sinais.Inteiros(16, Modular.Primo - 1, 7);
        var volta = Modular.Inverter(Modular.Transformar(vetor));
        Assert.Equal(vetor, volta);
    }

    /// <summary>
    /// A transformada inteira tem a MESMA estrutura da complexa: o impulso vira
    /// constante igual a um, e a constante vira n na posicao zero.
    /// </summary>
    [Fact]
    public void AInteiraTemAsMesmasPropriedadesDaComplexa()
    {
        var impulso = new long[8];
        impulso[0] = 1;
        Assert.All(Modular.Transformar(impulso), v => Assert.Equal(1, v));

        var constante = Enumerable.Repeat(1L, 8).ToArray();
        var espectro = Modular.Transformar(constante);
        Assert.Equal(8, espectro[0]);
        for (var k = 1; k < 8; k++) Assert.Equal(0, espectro[k]);
    }

    [Fact]
    public void ARaizDaUnidadeDoCorpoTemAOrdemCerta()
    {
        foreach (var n in new[] { 2, 8, 1024 })
        {
            var raiz = Modular.Potencia(Modular.Raiz, (Modular.Primo - 1) / n);
            Assert.Equal(1, Modular.Potencia(raiz, n));
            Assert.NotEqual(1, Modular.Potencia(raiz, n / 2));
        }
    }

    [Fact]
    public void OInversoDesfazAMultiplicacao()
    {
        foreach (var a in new long[] { 2, 3, 12345, Modular.Primo - 1 })
            Assert.Equal(1, a * Modular.Inverso(a) % Modular.Primo);
    }
}
