using Conde.Fourier;
using Xunit;

namespace Conde.Fourier.Testes;

/// <summary>
/// O juiz testado antes de julgar. Se a transformada pela definicao estiver
/// errada, todo o resto do repositorio concorda com ela e tudo passa.
/// </summary>
public class DefinicaoTestes
{
    private const double Folga = 1e-9;

    /// <summary>
    /// A transformada do IMPULSO e constante igual a um em toda frequencia. E o
    /// teste que da para conferir de cabeca, e ele pega sinal trocado, escala
    /// errada e ordem de bits invertida de uma vez.
    /// </summary>
    [Fact]
    public void OImpulsoTemEspectroConstante()
    {
        var espectro = Direta.Transformar(Sinais.Impulso(8));
        foreach (var c in espectro)
        {
            Assert.Equal(1, c.Real, Folga);
            Assert.Equal(0, c.Imaginario, Folga);
        }
    }

    /// <summary>
    /// E a CONSTANTE tem espectro de impulso: n na frequencia zero e nada no
    /// resto. Os dois juntos conferem os dois extremos da escala.
    /// </summary>
    [Fact]
    public void AConstanteTemEspectroDeImpulso()
    {
        var espectro = Direta.Transformar(Sinais.Constante(8));
        Assert.Equal(8, espectro[0].Real, Folga);
        for (var k = 1; k < 8; k++) Assert.Equal(0, espectro[k].Modulo, Folga);
    }

    /// <summary>
    /// Um cosseno com numero INTEIRO de ciclos na janela tem exatamente duas
    /// posicoes diferentes de zero, cada uma valendo n/2.
    /// </summary>
    [Fact]
    public void OCossenoInteiroTemDuasRaias()
    {
        const int n = 16;
        var espectro = Direta.Transformar(Sinais.Cosseno(n, 3));

        Assert.Equal(n / 2.0, espectro[3].Modulo, Folga);
        Assert.Equal(n / 2.0, espectro[n - 3].Modulo, Folga);
        for (var k = 0; k < n; k++)
            if (k != 3 && k != n - 3) Assert.Equal(0, espectro[k].Modulo, Folga);
    }

    [Fact]
    public void AIdaEVoltaDevolveOSinal()
    {
        var sinal = Sinais.Sorteado(16, 1);
        var volta = Direta.Inverter(Direta.Transformar(sinal));
        Assert.True(Complexo.MaiorDiferenca(sinal, volta) < Folga);
    }

    /// <summary>
    /// PARSEVAL: a energia do sinal e a energia do espectro dividida por n.
    ///
    /// E uma conferencia independente da ida e volta, porque ela olha so a
    /// saida, sem desfazer nada. Um erro de escala que a ida e volta cancelaria
    /// aparece aqui.
    /// </summary>
    [Fact]
    public void AEnergiaSeConservaComoParsevalMandou()
    {
        var sinal = Sinais.Sorteado(32, 2);
        var espectro = Direta.Transformar(sinal);
        Assert.Equal(Sinais.Energia(sinal), Sinais.Energia(espectro) / 32, 1e-9);
    }

    /// <summary>
    /// Deslocar o sinal no tempo multiplica o espectro por uma fase e nao muda
    /// NENHUM modulo. E a propriedade que faz o modulo do espectro servir para
    /// reconhecer coisa sem ligar para onde ela comeca.
    /// </summary>
    [Fact]
    public void DeslocarNoTempoNaoMudaOsModulos()
    {
        var sinal = Sinais.Sorteado(16, 4);
        var deslocado = new Complexo[16];
        for (var i = 0; i < 16; i++) deslocado[(i + 5) % 16] = sinal[i];

        var a = Direta.Transformar(sinal);
        var b = Direta.Transformar(deslocado);
        for (var k = 0; k < 16; k++) Assert.Equal(a[k].Modulo, b[k].Modulo, 1e-9);
    }

    /// <summary>
    /// Um sinal REAL tem espectro com simetria conjugada: a posicao k e o
    /// conjugado da posicao n-k. E por isso que metade do espectro de audio e
    /// jogada fora sem perder nada.
    /// </summary>
    [Fact]
    public void SinalRealTemEspectroSimetrico()
    {
        var espectro = Direta.Transformar(Complexo.De(3, 1, 4, 1, 5, 9, 2, 6));
        for (var k = 1; k < 8; k++)
        {
            Assert.Equal(espectro[k].Real, espectro[8 - k].Real, 1e-9);
            Assert.Equal(espectro[k].Imaginario, -espectro[8 - k].Imaginario, 1e-9);
        }
    }

    /// <summary>
    /// O sinal do expoente precisa ser o mesmo nos dois lados. Este teste
    /// confere o valor, nao so o modulo: um sinal trocado na ida e nao na volta
    /// devolve o sinal INVERTIDO NO TEMPO, que passa em teste de modulo.
    /// </summary>
    [Fact]
    public void AIdaEVoltaNaoInverteOTempo()
    {
        var sinal = Complexo.De(1, 2, 3, 4);
        var volta = Direta.Inverter(Direta.Transformar(sinal));

        Assert.Equal(1, volta[0].Real, Folga);
        Assert.Equal(2, volta[1].Real, Folga);
        Assert.Equal(3, volta[2].Real, Folga);
        Assert.Equal(4, volta[3].Real, Folga);
    }

    [Fact]
    public void AMultiplicacaoComplexaEstaCerta()
    {
        var produto = new Complexo(1, 2) * new Complexo(3, 4);
        Assert.Equal(-5, produto.Real, Folga);
        Assert.Equal(10, produto.Imaginario, Folga);
    }

    [Fact]
    public void AVoltaInteiraDaRaizEUm()
    {
        for (var n = 1; n <= 8; n++)
        {
            Assert.Equal(1, Complexo.Raiz(n, n).Real, 1e-12);
            Assert.Equal(0, Complexo.Raiz(n, n).Imaginario, 1e-12);
        }
    }
}
