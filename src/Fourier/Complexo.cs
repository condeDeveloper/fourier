namespace Conde.Fourier;

/// <summary>
/// Um numero complexo, escrito a mao.
///
/// O .NET tem System.Numerics.Complex e ele serviria. Escrever este aqui custa
/// trinta linhas e paga por si: a multiplicacao complexa e QUATRO
/// multiplicacoes reais e duas somas, e esse numero e a unidade em que o
/// repositorio inteiro mede. Com o tipo da biblioteca, a conta de operacoes
/// viraria chute.
/// </summary>
public readonly struct Complexo : IEquatable<Complexo>
{
    public Complexo(double real, double imaginario)
    {
        Real = real;
        Imaginario = imaginario;
    }

    public double Real { get; }
    public double Imaginario { get; }

    public static Complexo Zero => new(0, 0);
    public static Complexo Um => new(1, 0);

    /// <summary>
    /// A RAIZ DA UNIDADE: e elevado a (2*pi*i*k/n), que e o ponto do circulo
    /// unitario na fracao k/n da volta.
    ///
    /// O sinal do expoente e a convencao que mais gera confusao no assunto.
    /// Aqui a transformada direta usa expoente NEGATIVO e a inversa positivo,
    /// que e a convencao de engenharia. A escolha so importa por ser a mesma nos
    /// dois lados: trocar uma e nao a outra faz a ida e volta devolver o sinal
    /// INVERTIDO NO TEMPO, que e um erro que passa em teste de modulo e quebra
    /// em teste de valor.
    /// </summary>
    public static Complexo Raiz(int k, int n) =>
        new(Math.Cos(2 * Math.PI * k / n), Math.Sin(2 * Math.PI * k / n));

    public static Complexo operator +(Complexo a, Complexo b) =>
        new(a.Real + b.Real, a.Imaginario + b.Imaginario);

    public static Complexo operator -(Complexo a, Complexo b) =>
        new(a.Real - b.Real, a.Imaginario - b.Imaginario);

    public static Complexo operator *(Complexo a, Complexo b) =>
        new(a.Real * b.Real - a.Imaginario * b.Imaginario,
            a.Real * b.Imaginario + a.Imaginario * b.Real);

    public static Complexo operator *(Complexo a, double k) => new(a.Real * k, a.Imaginario * k);

    public static Complexo operator -(Complexo a) => new(-a.Real, -a.Imaginario);

    public Complexo Conjugado => new(Real, -Imaginario);

    public double Modulo => Math.Sqrt(Real * Real + Imaginario * Imaginario);

    public double Distancia(Complexo outro) => (this - outro).Modulo;

    public bool Equals(Complexo outro) => Real.Equals(outro.Real) && Imaginario.Equals(outro.Imaginario);

    public override bool Equals(object? obj) => obj is Complexo outro && Equals(outro);

    public override int GetHashCode() => HashCode.Combine(Real, Imaginario);

    public static bool operator ==(Complexo a, Complexo b) => a.Equals(b);

    public static bool operator !=(Complexo a, Complexo b) => !a.Equals(b);

    public override string ToString() =>
        Math.Abs(Imaginario) < 1e-12
            ? Real.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)
            : $"{Real.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)}" +
              $"{(Imaginario < 0 ? "-" : "+")}" +
              $"{Math.Abs(Imaginario).ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)}i";

    /// <summary>Transforma numeros reais em complexos, que e a entrada mais comum.</summary>
    public static Complexo[] De(params double[] reais)
    {
        ArgumentNullException.ThrowIfNull(reais);
        return [.. reais.Select(r => new Complexo(r, 0))];
    }

    /// <summary>A maior distancia entre dois vetores, item a item.</summary>
    public static double MaiorDiferenca(IReadOnlyList<Complexo> a, IReadOnlyList<Complexo> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count != b.Count) throw new ArgumentException("tamanhos diferentes", nameof(b));

        double pior = 0;
        for (var i = 0; i < a.Count; i++) pior = Math.Max(pior, a[i].Distancia(b[i]));
        return pior;
    }
}
