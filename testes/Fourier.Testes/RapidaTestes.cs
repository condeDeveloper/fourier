using Conde.Fourier;
using Xunit;

namespace Conde.Fourier.Testes;

public class RapidaTestes
{
    [Fact]
    public void ARapidaConcordaComADefinicaoEmTodoTamanho()
    {
        foreach (var n in new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 })
        {
            var sinal = Sinais.Sorteado(n, (ulong)n);
            var certo = Direta.Transformar(sinal);
            var rapido = Rapida.Transformar(sinal);
            Assert.True(Complexo.MaiorDiferenca(certo, rapido) < 1e-10, $"n = {n}");
        }
    }

    [Fact]
    public void ARapidaConcordaComADefinicaoNaVolta()
    {
        foreach (var n in new[] { 2, 8, 64, 256 })
        {
            var espectro = Sinais.Sorteado(n, (ulong)(n + 1));
            Assert.True(Complexo.MaiorDiferenca(Direta.Inverter(espectro), Rapida.Inverter(espectro)) < 1e-10);
        }
    }

    [Fact]
    public void TamanhoQueNaoEPotenciaDeDoisEhRecusado()
    {
        Assert.Throws<ArgumentException>(() => Rapida.Transformar(Sinais.Sorteado(6, 1)));
        Assert.Throws<ArgumentException>(() => Rapida.Transformar(Sinais.Sorteado(100, 1)));
    }

    [Fact]
    public void ATransformadaNaoMexeNaEntrada()
    {
        var sinal = Complexo.De(1, 2, 3, 4);
        var copia = (Complexo[])sinal.Clone();
        Rapida.Transformar(sinal);
        Assert.Equal(copia, sinal);
    }

    /// <summary>
    /// A inversao de bits e INVOLUTIVA: aplicar duas vezes volta ao original.
    /// A condicao "so troca quando i e menor" existe por isso, e sem ela cada
    /// par seria trocado duas vezes e nada aconteceria.
    /// </summary>
    [Fact]
    public void AInversaoDeBitsAplicadaDuasVezesNaoFazNada()
    {
        var vetor = Enumerable.Range(0, 16).ToArray();
        var copia = (int[])vetor.Clone();

        Bits.Reordenar(vetor);
        Assert.NotEqual(copia, vetor);

        Bits.Reordenar(vetor);
        Assert.Equal(copia, vetor);
    }

    [Fact]
    public void AInversaoDeBitsConfereNaMao()
    {
        Assert.Equal(0, Bits.Inverter(0, 3));
        Assert.Equal(4, Bits.Inverter(1, 3));
        Assert.Equal(2, Bits.Inverter(2, 3));
        Assert.Equal(6, Bits.Inverter(3, 3));
        Assert.Equal(7, Bits.Inverter(7, 3));
    }

    [Fact]
    public void OsExtremosFicamParados()
    {
        var vetor = Enumerable.Range(0, 8).ToArray();
        Bits.Reordenar(vetor);
        Assert.Equal(0, vetor[0]);
        Assert.Equal(7, vetor[7]);
    }

    [Fact]
    public void AProximaPotenciaConfere()
    {
        Assert.Equal(1, Bits.ProximaPotencia(1));
        Assert.Equal(8, Bits.ProximaPotencia(5));
        Assert.Equal(8, Bits.ProximaPotencia(8));
        Assert.Equal(16, Bits.ProximaPotencia(9));
    }

    [Fact]
    public void ZeroNaoEPotenciaDeDois()
    {
        Assert.False(Bits.PotenciaDeDois(0));
        Assert.False(Bits.PotenciaDeDois(-4));
        Assert.True(Bits.PotenciaDeDois(1));
    }

    /// <summary>
    /// O achado principal, travado: a transformada rapida erra MENOS que a
    /// definicao, e a vantagem cresce com o tamanho.
    /// </summary>
    [Fact]
    public void ARapidaErraMenosQueADefinicao()
    {
        double anterior = 0;
        foreach (var n in new[] { 128, 512, 2048 })
        {
            var sinal = Sinais.Sorteado(n, 3);
            var pelaDefinicao = Complexo.MaiorDiferenca(sinal, Direta.Inverter(Direta.Transformar(sinal)));
            var pelaRapida = Complexo.MaiorDiferenca(sinal, Rapida.Inverter(Rapida.Transformar(sinal)));

            Assert.True(pelaRapida < pelaDefinicao, $"n = {n}");

            var vantagem = pelaDefinicao / pelaRapida;
            Assert.True(vantagem > anterior, "a vantagem precisa crescer com o tamanho");
            anterior = vantagem;
        }
    }

    [Fact]
    public void AContaDeMultiplicacoesConfere()
    {
        Assert.Equal(12, Rapida.Multiplicacoes(8));
        Assert.Equal(64, Direta.Multiplicacoes(8));
        Assert.Equal(0, Rapida.Multiplicacoes(1));
    }

    [Fact]
    public void AEnergiaSeConservaNaRapidaTambem()
    {
        var sinal = Sinais.Sorteado(64, 9);
        var espectro = Rapida.Transformar(sinal);
        Assert.Equal(Sinais.Energia(sinal), Sinais.Energia(espectro) / 64, 1e-9);
    }
}
