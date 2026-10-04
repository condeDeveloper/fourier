# fourier

Transformada de Fourier do zero em C# e .NET 8: a definição, Cooley-Tukey,
Bluestein para tamanho qualquer, e a transformada em aritmética inteira. O juiz
é a definição, lenta de propósito, e a transformada inteira, que não usa um único
número de ponto flutuante.

```
$ dotnet medidor.dll erro

         n       definicao          rapida     quem erra menos
         8       7.45E-016       1.24E-016        a rapida, 6x
        32       8.06E-015       3.61E-016       a rapida, 22x
       128       3.84E-014       5.55E-016       a rapida, 69x
       512       1.75E-013       7.78E-016      a rapida, 225x
      2048       8.23E-013       1.01E-015      a rapida, 819x
      8192       3.46E-012       1.34E-015    a rapida, 2,582x
```

Este é o resultado que eu esperava ao contrário, e está mais abaixo.

## O juiz é a definição

A transformada pela definição não tem truque, não tem recursão, não tem embaralho
de índice, e é lenta de propósito: cada linha é a fórmula copiada do papel.

Isso importa porque as transformadas rápidas erram de um jeito **traiçoeiro**. Um
expoente com sinal trocado, uma ordem de bits invertida na direção errada ou um
fator de escala fora do lugar produzem uma saída com o mesmo tamanho, os mesmos
módulos e a mesma cara de espectro. Nenhum teste de "parece certo" pega isso, e é
por isso que os testes daqui conferem **valor** e não módulo.

E há um segundo juiz, exato: a transformada em aritmética inteira módulo
998244353. Transformada rápida precisa de raízes da unidade, e raiz da unidade é
uma propriedade **algébrica**, não geométrica: basta um número g com g^n = 1. Num
corpo de restos módulo um primo bem escolhido esses números existem, e a
transformada roda inteira, com a mesma inversão de bits e as mesmas borboletas.

## O que as medidas me corrigiram

**A transformada rápida erra MENOS que a definição.** Eu ia escrever o contrário:
ela faz menos contas, e o resultado de cada borboleta alimenta a próxima, então o
erro se acumula em cadeia. Ela erra menos, e por ordens de grandeza — 2.582 vezes
menos com 8192 pontos, e a vantagem cresce com o tamanho.

O motivo é que o erro de ponto flutuante cresce com a **profundidade** da cadeia
de contas, não com a quantidade. Na definição, cada saída é uma soma de n termos
e os erros de n parcelas se empilham um atrás do outro. Na rápida, cada saída
passa por log2(n) níveis, e cada nível soma dois números. Com 8192 pontos são
8192 somas em cadeia contra 13.

**A transformada "exata" quebra ANTES da aproximada.**

```
$ dotnet medidor.dll precisao

   tamanho   maior coeficiente         maior resultado    complexa    inteira
       256                 256               4,481,536       exata      exata
       256               4,096           1,110,895,766       exata       ERRA
       256              65,536         267,211,703,656       exata       ERRA
       256           1,048,576      79,382,711,300,571       exata       ERRA
       256          16,777,216  18,547,853,413,465,335        ERRA       ERRA
```

As duas quebram, por motivos opostos e em pontos bem diferentes. A complexa
quebra por **arredondamento**: o resultado certo é inteiro, o calculado não é, e
enquanto o erro fica abaixo de meio o arredondamento conserta tudo; passado isso,
ele conserta para o número errado. A inteira quebra por **transbordo**: ela é
exata módulo 998244353, e quando o coeficiente certo passa disso o que volta é o
resto.

Nenhuma das duas avisa. É por isso que a biblioteca tem a definição junto: para
tamanho pequeno ela confere, e a conferência é o teste.

**A onda quadrada tem harmônico ímpar, e eu procurei no lugar errado.** Com 64
pontos e meio período de 8, a fundamental cai na posição 4, e os harmônicos
ímpares são 3, 5 e 7 vezes a **fundamental**: posições 12, 20 e 28, que são
números pares. Eu tinha escrito "as posições pares estão vazias", e as posições
cheias são justamente pares. Ímpar é o múltiplo, não o índice.

**O ângulo de Bluestein precisa do resto.** A fase é `pi*j*j/n`, e com n = 1000 e
j = 999 o `j*j` passa de um milhão. O cosseno de um ângulo enorme perde quase
todos os dígitos, porque a redução interna trabalha com um pi de precisão finita.
Tirar o resto por 2n antes não é otimização: é a diferença entre 1e-9 e lixo.

## Completar com zeros não é a mesma transformada

```
$ dotnet medidor.dll tamanho

       n   bluestein x definicao    zeros ate a potencia    multiplicacoes
       3               2.22E-016               1.07E+000                50
       7               4.22E-015               3.66E+000               126
     100               3.66E-013               2.31E+001             3,528
    1000               2.00E-011               1.01E+002            37,840
```

A coluna do meio é o ponto. A transformada de 100 pontos completada para 128 tem
128 saídas, em frequências que **não** são as 100 originais, e o número da posição
k deixa de ser o número da posição k. Para alguns usos isso não importa; para
comparar com a transformada de verdade, importa todo.

A única posição que sobrevive é a frequência zero, porque ela é só a soma do
sinal e os zeros não somam nada.

O truque de Bluestein, de 1970, devolve a transformada verdadeira de qualquer n,
usando uma identidade de colégio: `j*k = (j*j + k*k - (k-j)^2)/2`. Com ela a soma
da transformada vira uma **convolução**, e convolução se faz com transformadas de
tamanho potência de dois. O preço são três transformadas de tamanho pelo menos
2n-1 em vez de uma de n.

## Vazamento não é erro numérico

```
$ dotnet medidor.dll vazamento

  ciclos na janela    posicoes acima de 1%    maior fora do pico
             10.00                       2                0.00 %
             10.25                      73                8.86 %
             10.50                     256               21.60 %
             10.75                      89               11.40 %
             11.00                       2                0.00 %
```

Com um número inteiro de ciclos na janela, o espectro tem duas posições e mais
nada. Com meio ciclo a mais, tem todas as 256, e a energia aparece em frequências
que não existem no sinal.

A causa não é arredondamento: a transformada pela definição vaza exatamente
igual, e um teste confere isso. A transformada supõe que o sinal se repete fora da
janela, e um cosseno de 10,5 ciclos repetido tem um **degrau** na emenda. O
espectro está certo: ele é o espectro do sinal com o degrau, que é o sinal que a
transformada está vendo.

## O custo

```
$ dotnet medidor.dll custo

         n       definicao        rapida         razao
      1024       1,048,576         5,120           205
     65536   4,294,967,296       524,288         8,192
   1048576 1,099,511,627,776    10,485,760       104,858
```

Gauss escreveu este algoritmo em 1805, para interpolar a órbita do asteroide
Pallas. O texto só foi publicado depois da morte dele, em latim, e ninguém
percebeu. Cento e sessenta anos depois Cooley e Tukey o redescobriram, e ele virou
um dos algoritmos mais usados do mundo.

## As peças

| arquivo | o que faz |
| --- | --- |
| `Complexo.cs` | o complexo escrito à mão, para a conta de multiplicações ser exata |
| `Direta.cs` | a fórmula copiada do papel, que é o juiz |
| `Bits.cs` | a inversão de bits, que é a parte que ninguém explica |
| `Rapida.cs` | Cooley-Tukey iterativo, e a inversa pelo truque do conjugado |
| `Bluestein.cs` | tamanho qualquer, pela identidade que vira convolução |
| `Modular.cs` | a mesma transformada sem um único ponto flutuante |
| `Convolucao.cs` | os três caminhos, e onde cada um deixa de concordar |
| `Sinais.cs` | impulso, constante, cosseno, quadrada e o sorteio com semente |

## Como rodar

```
dotnet test testes/Fourier.Testes/Fourier.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

As medidas aceitam `custo`, `erro`, `precisao`, `tamanho`, `vazamento` e `tudo`.

## Licença

MIT.
