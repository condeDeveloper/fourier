using Conde.Fourier;
using Xunit;

namespace Conde.Fourier.Testes;

/// <summary>
/// O vazamento espectral, que nao e erro numerico e parece erro numerico.
/// </summary>
public class VazamentoTestes
{
    private const int N = 256;

    private static double[] Modulos(double ciclos) =>
        [.. Rapida.Transformar(Sinais.Cosseno(N, ciclos)).Select(c => c.Modulo)];

    /// <summary>
    /// Com numero INTEIRO de ciclos na janela, o espectro tem duas posicoes e
    /// mais nada.
    /// </summary>
    [Fact]
    public void OCicloInteiroDaEspectroLimpo()
    {
        var modulos = Modulos(10);
        var pico = modulos.Max();

        Assert.Equal(2, modulos.Count(m => m > pico / 100));
        Assert.Equal(pico, modulos[10], 1e-9);
        Assert.Equal(pico, modulos[N - 10], 1e-9);
    }

    /// <summary>
    /// Com meio ciclo a mais, a energia aparece em TODA frequencia, inclusive em
    /// frequencias que o sinal nao tem.
    /// </summary>
    [Fact]
    public void OMeioCicloVazaParaTodoLado()
    {
        var modulos = Modulos(10.5);
        var pico = modulos.Max();
        Assert.Equal(N, modulos.Count(m => m > pico / 100));
    }

    /// <summary>
    /// O pior caso e exatamente no meio do caminho entre duas posicoes, e o
    /// vazamento diminui conforme a frequencia se aproxima de uma posicao
    /// inteira.
    /// </summary>
    [Fact]
    public void OPiorCasoEhNoMeioDoCaminho()
    {
        Assert.True(Fora(10.5) > Fora(10.25));
        Assert.True(Fora(10.25) > Fora(10.05));
        Assert.True(Fora(10.05) > Fora(10.0));
    }

    private static double Fora(double ciclos)
    {
        var modulos = Modulos(ciclos);
        var pico = modulos.Max();
        var fora = modulos.Select((m, i) => (m, i))
                          .Where(p => Math.Abs(p.i - 10) > 2 && Math.Abs(p.i - (N - 10)) > 2)
                          .Max(p => p.m);
        return fora / pico;
    }

    /// <summary>
    /// E a conferencia que prova que nao e erro numerico: a transformada pela
    /// DEFINICAO, que nao tem borboleta nenhuma, mostra exatamente o mesmo
    /// vazamento.
    /// </summary>
    [Fact]
    public void ADefinicaoVazaIgual()
    {
        var sinal = Sinais.Cosseno(64, 10.5);
        var pelaDefinicao = Direta.Transformar(sinal);
        var pelaRapida = Rapida.Transformar(sinal);
        Assert.True(Complexo.MaiorDiferenca(pelaDefinicao, pelaRapida) < 1e-10);

        var pico = pelaDefinicao.Max(c => c.Modulo);
        Assert.True(pelaDefinicao.Count(c => c.Modulo > pico / 100) > 10);
    }

    /// <summary>
    /// A energia continua a mesma: o vazamento nao cria nem destroi nada, ele
    /// REPARTE o que havia.
    /// </summary>
    [Fact]
    public void OVazamentoNaoCriaEnergia()
    {
        foreach (var ciclos in new[] { 10.0, 10.5 })
        {
            var sinal = Sinais.Cosseno(N, ciclos);
            var espectro = Rapida.Transformar(sinal);
            Assert.Equal(Sinais.Energia(sinal), Sinais.Energia(espectro) / N, 1e-8);
        }
    }

    /// <summary>
    /// A onda quadrada so tem harmonico IMPAR, e o teste que eu escrevi primeiro
    /// procurava isso no lugar errado.
    ///
    /// Com 64 pontos e meio periodo de 8, o periodo e 16 e a fundamental cai na
    /// posicao 64/16 = 4. Os harmonicos impares sao 3, 5 e 7 vezes a
    /// FUNDAMENTAL, ou seja, as posicoes 12, 20 e 28, que sao numeros pares. Eu
    /// tinha escrito "as posicoes pares estao vazias" e as posicoes cheias sao
    /// justamente pares: impar e o multiplo, nao o indice.
    ///
    /// E a razao entre o terceiro harmonico e a fundamental e um terco, que e a
    /// serie da onda quadrada aparecendo sozinha na saida.
    /// </summary>
    [Fact]
    public void AOndaQuadradaSoTemHarmonicoImparDaFundamental()
    {
        var espectro = Rapida.Transformar(Sinais.Quadrada(64, 8));
        var pico = espectro.Max(c => c.Modulo);

        const int fundamental = 4;
        Assert.Equal(pico, espectro[fundamental].Modulo, 1e-9);

        for (var k = 1; k < 32; k++)
        {
            var multiplo = k / (double)fundamental;
            var ehHarmonicoImpar = k % fundamental == 0 && (int)multiplo % 2 == 1;
            if (ehHarmonicoImpar) continue;
            Assert.True(espectro[k].Modulo < pico / 1000, $"a posicao {k} deveria estar vazia");
        }

        var terceiro = espectro[3 * fundamental].Modulo / pico;
        Assert.InRange(terceiro, 0.33, 0.36);
    }

    [Fact]
    public void OSorteioEhReproduzivel()
    {
        Assert.Equal(Sinais.Sorteado(16, 42), Sinais.Sorteado(16, 42));
        Assert.NotEqual(Sinais.Sorteado(16, 42), Sinais.Sorteado(16, 43));
    }

    [Fact]
    public void OsInteirosSorteadosFicamNaFaixa()
    {
        var valores = Sinais.Inteiros(100, 50, 7);
        Assert.All(valores, v => Assert.InRange(v, 0, 50));
    }
}
