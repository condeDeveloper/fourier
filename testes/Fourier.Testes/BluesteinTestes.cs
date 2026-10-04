using Conde.Fourier;
using Xunit;

namespace Conde.Fourier.Testes;

public class BluesteinTestes
{
    [Fact]
    public void ConcordaComADefinicaoEmTamanhoQualquer()
    {
        foreach (var n in new[] { 1, 2, 3, 5, 6, 7, 9, 10, 11, 12, 13, 15, 17, 100, 101 })
        {
            var sinal = Sinais.Sorteado(n, (ulong)n);
            var certo = Direta.Transformar(sinal);
            var pelo = Bluestein.Transformar(sinal);
            Assert.True(Complexo.MaiorDiferenca(certo, pelo) < 1e-9, $"n = {n}");
        }
    }

    [Fact]
    public void FuncionaTambemEmPotenciaDeDois()
    {
        foreach (var n in new[] { 2, 4, 8, 64 })
        {
            var sinal = Sinais.Sorteado(n, 5);
            Assert.True(Complexo.MaiorDiferenca(Rapida.Transformar(sinal), Bluestein.Transformar(sinal)) < 1e-10);
        }
    }

    [Fact]
    public void AIdaEVoltaDevolveOSinal()
    {
        foreach (var n in new[] { 3, 7, 12, 100 })
        {
            var sinal = Sinais.Sorteado(n, (ulong)(n + 7));
            var volta = Bluestein.Inverter(Bluestein.Transformar(sinal));
            Assert.True(Complexo.MaiorDiferenca(sinal, volta) < 1e-9, $"n = {n}");
        }
    }

    [Fact]
    public void OImpulsoDeTamanhoImparTambemDaEspectroConstante()
    {
        foreach (var n in new[] { 3, 5, 7, 11 })
            foreach (var c in Bluestein.Transformar(Sinais.Impulso(n)))
            {
                Assert.Equal(1, c.Real, 1e-9);
                Assert.Equal(0, c.Imaginario, 1e-9);
            }
    }

    /// <summary>
    /// A saida obvia para tamanho estranho, completar com zeros, da OUTRA
    /// transformada. O teste exibe a diferenca em vez de a explicar.
    /// </summary>
    [Fact]
    public void CompletarComZerosDaOutraTransformada()
    {
        const int n = 12;
        var sinal = Sinais.Sorteado(n, 5);
        var certo = Direta.Transformar(sinal);

        var completado = new Complexo[16];
        for (var i = 0; i < n; i++) completado[i] = sinal[i];
        var porZeros = Rapida.Transformar(completado);

        Assert.True(Complexo.MaiorDiferenca(certo, Bluestein.Transformar(sinal)) < 1e-9);

        var maiorDiferenca = certo.Select((c, i) => c.Distancia(porZeros[i])).Max();
        Assert.True(maiorDiferenca > 1, "completar com zeros deveria dar outra coisa");
    }

    /// <summary>
    /// A frequencia ZERO e a unica que sobrevive a completar com zeros, porque
    /// ela e so a soma do sinal e os zeros nao somam nada.
    /// </summary>
    [Fact]
    public void SoAFrequenciaZeroSobreviveAOsZeros()
    {
        const int n = 12;
        var sinal = Sinais.Sorteado(n, 5);
        var certo = Direta.Transformar(sinal);

        var completado = new Complexo[16];
        for (var i = 0; i < n; i++) completado[i] = sinal[i];
        var porZeros = Rapida.Transformar(completado);

        Assert.True(certo[0].Distancia(porZeros[0]) < 1e-9);
    }

    /// <summary>
    /// O angulo usa j*j modulo 2n, e nao j*j direto. Com n grande, j*j passa de
    /// um milhao e o cosseno de um angulo enorme perde quase todos os digitos.
    /// </summary>
    [Fact]
    public void ATransformadaGrandeNaoPerdePrecisaoNoAngulo()
    {
        const int n = 1000;
        var sinal = Sinais.Sorteado(n, 5);
        Assert.True(Complexo.MaiorDiferenca(Direta.Transformar(sinal), Bluestein.Transformar(sinal)) < 1e-9);
    }

    [Fact]
    public void AContaDeMultiplicacoesCresceComOTamanhoCompletado()
    {
        Assert.True(Bluestein.Multiplicacoes(100) > Rapida.Multiplicacoes(128));
        Assert.Equal(0, Bluestein.Multiplicacoes(1));
    }

    [Fact]
    public void EnergiaSeConservaEmTamanhoQualquer()
    {
        foreach (var n in new[] { 3, 7, 13 })
        {
            var sinal = Sinais.Sorteado(n, 2);
            var espectro = Bluestein.Transformar(sinal);
            Assert.Equal(Sinais.Energia(sinal), Sinais.Energia(espectro) / n, 1e-9);
        }
    }
}
