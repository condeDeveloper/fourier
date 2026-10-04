using Conde.Fourier;

namespace Conde.Fourier.Medidor;

/// <summary>
/// As medidas. Contagem de multiplicacoes e ERRO MEDIDO, nunca relogio: o
/// numero que sai aqui e o mesmo em qualquer maquina.
/// </summary>
public static class Programa
{
    public static int Main(string[] argumentos)
    {
        var qual = argumentos.Length > 0 ? argumentos[0] : "tudo";
        switch (qual)
        {
            case "custo": Custo(); break;
            case "erro": Erro(); break;
            case "precisao": Precisao(); break;
            case "tamanho": Tamanho(); break;
            case "vazamento": Vazamento(); break;
            case "tudo": Custo(); Erro(); Precisao(); Tamanho(); Vazamento(); break;
            default:
                Console.Error.WriteLine("medidas: custo, erro, precisao, tamanho, vazamento, tudo");
                return 1;
        }
        return 0;
    }

    /// <summary>Quantas multiplicacoes cada caminho gasta.</summary>
    private static void Custo()
    {
        Console.WriteLine("== multiplicacoes complexas: definicao contra rapida ==");
        Console.WriteLine($"{"n",10}{"definicao",16}{"rapida",14}{"razao",14}");

        foreach (var n in new[] { 8, 64, 1024, 4096, 65536, 1 << 20 })
            Console.WriteLine($"{n,10}{Direta.Multiplicacoes(n),16:N0}{Rapida.Multiplicacoes(n),14:N0}" +
                              $"{Direta.Multiplicacoes(n) / (double)Rapida.Multiplicacoes(n),14:N0}");

        Console.WriteLine();
        Console.WriteLine("com um milhao de pontos a diferenca e de cem mil vezes. E o numero que");
        Console.WriteLine("explica por que tomografia, compressao de audio e telefonia celular");
        Console.WriteLine("existem na forma em que existem");
        Console.WriteLine();
    }

    /// <summary>
    /// O erro de ida e volta das duas, que e o achado principal do repositorio.
    /// </summary>
    private static void Erro()
    {
        Console.WriteLine("== o erro de ida e volta: definicao contra rapida ==");
        Console.WriteLine($"{"n",10}{"definicao",16}{"rapida",16}{"quem erra menos",20}");

        foreach (var n in new[] { 8, 32, 128, 512, 2048, 8192 })
        {
            var sinal = Sinais.Sorteado(n, 3);
            var pelaDefinicao = Complexo.MaiorDiferenca(sinal, Direta.Inverter(Direta.Transformar(sinal)));
            var pelaRapida = Complexo.MaiorDiferenca(sinal, Rapida.Inverter(Rapida.Transformar(sinal)));

            Console.WriteLine($"{n,10}{pelaDefinicao,16:E2}{pelaRapida,16:E2}" +
                              $"{(pelaRapida < pelaDefinicao ? $"a rapida, {pelaDefinicao / pelaRapida:N0}x" : "a definicao"),20}");
        }

        Console.WriteLine();
        Console.WriteLine("este e o resultado que eu esperava ao contrario. A transformada rapida faz");
        Console.WriteLine("MENOS contas e eu ia escrever que ela paga isso em precisao, porque o");
        Console.WriteLine("resultado de cada borboleta alimenta a proxima e o erro se acumula em");
        Console.WriteLine("cadeia. Ela erra MENOS, e por ordens de grandeza");
        Console.WriteLine();
        Console.WriteLine("o motivo e que o erro de ponto flutuante cresce com a PROFUNDIDADE da");
        Console.WriteLine("cadeia de contas, nao com a quantidade. Na definicao, cada saida e uma");
        Console.WriteLine("soma de n termos, e os erros de n parcelas se empilham um atras do outro.");
        Console.WriteLine("Na rapida, cada saida passa por log2(n) niveis, e cada nivel soma dois");
        Console.WriteLine("numeros. Com 8192 pontos sao 8192 somas em cadeia contra 13");
        Console.WriteLine();
    }

    /// <summary>
    /// Ate onde da para confiar na convolucao por numero complexo, conferida
    /// contra a conta exata.
    /// </summary>
    private static void Precisao()
    {
        Console.WriteLine("== convolucao inteira: ate onde o ponto flutuante aguenta ==");
        Console.WriteLine($"{"tamanho",10}{"maior coeficiente",20}{"maior resultado",24}{"complexa",12}{"inteira",11}");

        foreach (var n in new[] { 64, 256, 1024 })
            foreach (var teto in new long[] { 1 << 8, 1 << 12, 1 << 16, 1 << 20, 1 << 24 })
            {
                var a = Sinais.Inteiros(n, teto, 11);
                var b = Sinais.Inteiros(n, teto, 22);

                var certo = Convolucao.Definicao(a, b);
                var complexa = Convolucao.PorComplexo(a, b);
                var inteira = Convolucao.PorInteiro(a, b);

                var maior = certo.Max();
                var complexaBate = certo.SequenceEqual(complexa);
                var inteiraBate = certo.SequenceEqual(inteira);

                Console.WriteLine($"{n,10}{teto,20:N0}{maior,24:N0}" +
                                  $"{(complexaBate ? "exata" : "ERRA"),12}{(inteiraBate ? "exata" : "ERRA"),11}");
            }

        Console.WriteLine();
        Console.WriteLine("as duas quebram, e por motivos opostos. A complexa quebra por ARREDONDAMENTO:");
        Console.WriteLine("o resultado certo e inteiro, o calculado nao e, e enquanto o erro fica");
        Console.WriteLine("abaixo de meio o arredondamento conserta tudo. Passado isso ele conserta");
        Console.WriteLine("para o numero errado, calado");
        Console.WriteLine();
        Console.WriteLine("a inteira quebra por TRANSBORDO: ela e exata modulo 998244353, e quando o");
        Console.WriteLine("coeficiente certo passa disso, o que volta e o resto. Tambem calado");
        Console.WriteLine();
        Console.WriteLine("nenhuma das duas avisa. E por isso que a biblioteca tem a definicao junto:");
        Console.WriteLine("para um tamanho pequeno ela confere, e a conferencia e o teste");
        Console.WriteLine();
    }

    /// <summary>
    /// O que acontece com tamanho que nao e potencia de dois: completar com zero
    /// nao e a mesma transformada.
    /// </summary>
    private static void Tamanho()
    {
        Console.WriteLine("== tamanho que nao e potencia de dois ==");
        Console.WriteLine($"{"n",8}{"bluestein x definicao",24}{"zeros ate a potencia",24}{"multiplicacoes",18}");

        foreach (var n in new[] { 3, 5, 7, 12, 100, 1000 })
        {
            var sinal = Sinais.Sorteado(n, 5);
            var certo = Direta.Transformar(sinal);
            var porBluestein = Bluestein.Transformar(sinal);

            // Completar com zeros ate a proxima potencia e a saida obvia, e da
            // OUTRA transformada: as frequencias deixam de ser as mesmas.
            var completado = new Complexo[Bits.ProximaPotencia(n)];
            for (var i = 0; i < n; i++) completado[i] = sinal[i];
            var porZeros = Rapida.Transformar(completado);

            var diferencaBluestein = Complexo.MaiorDiferenca(certo, porBluestein);
            var diferencaZeros = certo.Select((c, i) => c.Distancia(porZeros[i])).Max();

            Console.WriteLine($"{n,8}{diferencaBluestein,24:E2}{diferencaZeros,24:E2}" +
                              $"{Bluestein.Multiplicacoes(n),18:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("a coluna do meio e o ponto. Completar com zeros parece inofensivo e nao e:");
        Console.WriteLine("a transformada de 100 pontos completada para 128 tem 128 saidas, em");
        Console.WriteLine("frequencias que NAO sao as 100 originais, e o numero da posicao k deixa de");
        Console.WriteLine("ser o numero da posicao k. Para alguns usos isso nao importa; para");
        Console.WriteLine("comparar com a transformada de verdade, importa todo");
        Console.WriteLine();
        Console.WriteLine("o truque de Bluestein devolve a transformada verdadeira de qualquer n, e");
        Console.WriteLine("cobra tres transformadas de tamanho pelo menos 2n-1 em vez de uma de n");
        Console.WriteLine();
    }

    /// <summary>
    /// O vazamento espectral: a diferenca entre uma frequencia que cabe na
    /// janela e uma que nao cabe.
    /// </summary>
    private static void Vazamento()
    {
        Console.WriteLine("== vazamento: a frequencia que nao cabe na janela ==");
        Console.WriteLine($"{"ciclos na janela",18}{"posicoes acima de 1%",24}{"maior fora do pico",22}");

        const int n = 256;
        foreach (var ciclos in new[] { 10.0, 10.25, 10.5, 10.75, 11.0 })
        {
            var espectro = Rapida.Transformar(Sinais.Cosseno(n, ciclos));
            var modulos = espectro.Select(c => c.Modulo).ToArray();
            var pico = modulos.Max();

            var acimaDeUmPorCento = modulos.Count(m => m > pico / 100);
            var fora = modulos.Select((m, i) => (m, i))
                              .Where(p => Math.Abs(p.i - 10) > 2 && Math.Abs(p.i - (n - 10)) > 2)
                              .Max(p => p.m);

            Console.WriteLine($"{ciclos,18:N2}{acimaDeUmPorCento,24}{fora / pico,22:P2}");
        }

        Console.WriteLine();
        Console.WriteLine("com um numero INTEIRO de ciclos na janela, o espectro tem duas posicoes e");
        Console.WriteLine("mais nada. Com meio ciclo a mais, ele tem dezenas, e a energia aparece em");
        Console.WriteLine("frequencias que nao existem no sinal");
        Console.WriteLine();
        Console.WriteLine("a causa nao e erro numerico nenhum. A transformada supoe que o sinal se");
        Console.WriteLine("repete fora da janela, e um cosseno de 10,5 ciclos repetido tem um DEGRAU");
        Console.WriteLine("na emenda. O espectro esta certo: ele e o espectro do sinal com o degrau,");
        Console.WriteLine("que e o sinal que a transformada esta vendo");
        Console.WriteLine();
    }
}
