using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace calc_hack
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Calc HACK!");

                var movimentos = LerNumero("Informe a quantidade de movimentos: ");
                var esperado = LerNumero("Informe o valor esperado: ");
                var inicial = LerNumero("Informe o valor inicial: ");
                var operacoes = new List<Operacao>();

                Console.WriteLine("Operações possíveis: n, +n, -n, xn, /n, pn, sum, +/-, <<, rev, <, >");
                do
                {
                    Console.Write("Informe uma operação ([ENTER] para finalizar): ");
                    var textoOp = Console.ReadLine();
                    if (textoOp == "")
                    {
                        break;
                    }
                    try
                    {
                        operacoes.Add(new Operacao(textoOp));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                } while (true);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Parametros informados:");
                Console.WriteLine($"- Movimentos: {movimentos}");
                Console.WriteLine($"- Valor Esperado: {esperado}");
                Console.WriteLine($"- Valor Inicial: {inicial}");
                foreach (var operacao in operacoes)
                {
                    Console.WriteLine($"- Operação '{operacao.textoOperacao}' - Tipo: {operacao.tipo} - ValorA: {operacao.ValorA} - ValorB: {operacao.ValorB}");
                }

                Console.WriteLine("Executando cálculo");

                var totalpermutacoes = operacoes.Count;
                for (int i = 0; i < movimentos - 1; i++)
                {
                    totalpermutacoes *= operacoes.Count;
                }
                Console.WriteLine($"Total de Permutações: {totalpermutacoes}");

                var permutacoes = new int[totalpermutacoes, movimentos];
                for (int y = 0; y < movimentos; y++)
                {
                    var idxAtual = 0;
                    var qtMaxima = Math.Pow(operacoes.Count, y);
                    var qtEmitida = 0;
                    for (int i = 0; i < totalpermutacoes; i++)
                    {
                        permutacoes[i, y] = idxAtual;
                        qtEmitida++;
                        if (qtEmitida >= qtMaxima)
                        {
                            qtEmitida = 0;
                            idxAtual++;
                            if (idxAtual > operacoes.Count - 1)
                            {
                                idxAtual = 0;
                            }
                        }
                    }
                }

                var resultados = new List<Resultado>();

                //Executa operações até encontrar resultado
                for (int i = 0; i < totalpermutacoes; i++)
                {
                    var proxResultado = new Resultado();
                    proxResultado.idxPermutacao = i;
                    float valorAtual = inicial;
                    try
                    {
                        for (int y = 0; y < movimentos; y++)
                        {
                            var idxAtual = permutacoes[i, y];
                            proxResultado.sequencia += operacoes[idxAtual].textoOperacao + "  ";
                            proxResultado.qtMovimentos++;
                            valorAtual = operacoes[idxAtual].executarOperacao(valorAtual);
                            if (valorAtual == esperado)
                            {
                                proxResultado.resolvido = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        proxResultado.exception = true;
                        proxResultado.mensagem = ex.Message;
                        proxResultado.stacktrace = ex.StackTrace;

                    }
                    resultados.Add(proxResultado);
                }

                var resultadosFiltrados = new List<Resultado>();
                if (!resultados.Any(r => r.resolvido))
                {
                    resultadosFiltrados = resultados.Where(r => !r.resolvido && !r.exception).ToList();
                    Console.WriteLine(" -------------------------------------- ");
                    Console.WriteLine("Resultado SEM SUCESSO");
                    foreach (var resultado in resultadosFiltrados)
                    {
                        Console.WriteLine(" -  " + resultado.sequencia);
                    }
                    Console.WriteLine($"POSSIBILIDADES {resultadosFiltrados.Count}");
                    Console.WriteLine(" -------------------------------------- ");

                    Console.WriteLine();
                    Console.WriteLine();
                }

                if (!resultados.Any(r => r.resolvido) && resultados.Any(r => r.exception))
                {
                    resultadosFiltrados = resultados.Where(r => r.exception).ToList();
                    Console.WriteLine(" -------------------------------------- ");
                    Console.WriteLine("Resultado COM EXCEPTION");
                    foreach (var resultado in resultadosFiltrados)
                    {
                        Console.WriteLine(" -  " + resultado.sequencia);
                        Console.WriteLine("  ->" + resultado.mensagem);
                        Console.WriteLine("  ->" + resultado.stacktrace);
                    }
                    Console.WriteLine($"POSSIBILIDADES {resultadosFiltrados.Count}");
                    Console.WriteLine(" -------------------------------------- ");
                    Console.WriteLine();
                    Console.WriteLine();
                }


                resultadosFiltrados = resultados.Where(r => r.resolvido).OrderByDescending(r => r.qtMovimentos).ThenBy(r => r.idxPermutacao).ToList();
                Console.WriteLine(" -------------------------------------- ");
                Console.WriteLine("Resultado COM SUCESSO");

                foreach (var resultado in resultadosFiltrados)
                {
                    Console.WriteLine($"{resultado.idxPermutacao} ({resultado.qtMovimentos}) => {resultado.sequencia}");
                }
                Console.WriteLine($"POSSIBILIDADES {resultadosFiltrados.Count}");
                Console.WriteLine(" -------------------------------------- ");

            }
        }


        static int LerNumero(string enunciado)
        {
            var resultado = 0;
            while (true)
            {
                Console.Write(enunciado);
                var leitura = Console.ReadLine();
                if (int.TryParse(leitura, out resultado))
                {
                    break;
                }
                Console.WriteLine("!! Valor inválido. Informe novamente !!");
            }
            return resultado;
        }


    }

    class Resultado
    {
        public int idxPermutacao { get; set; }
        public string sequencia { get; set; }
        public string mensagem { get; set; }
        public string stacktrace { get; set; }
        public bool exception { get; set; }
        public bool resolvido { get; set; }
        public int qtMovimentos { get; set; }
    }

    class Operacao
    {
        private Regex rgxPotencia = new Regex(@"^p([-\d]+)$");
        private Regex rgxMultiplicacao = new Regex(@"^x([-\d]+)$");
        private Regex rgxDivisao = new Regex(@"^/([-\d]+)$");
        private Regex rgxSomar = new Regex(@"^\+(\d+)$");
        private Regex rgxSubtrair = new Regex(@"^-(\d+)$");
        private Regex rgxAddCaracter = new Regex(@"^\d+$");
        private Regex rgxSubstituiValor = new Regex(@"^(\d+)=>(\d+)$");

        public Operacao(string texto)
        {
            this.textoOperacao = texto;

            if (rgxPotencia.IsMatch(texto))
            {
                tipo = tipoOperacao.potencia;
                var mtc = rgxPotencia.Match(texto);
                var vlr = 0;
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxMultiplicacao.IsMatch(texto))
            {
                tipo = tipoOperacao.multiplicacao;
                var mtc = rgxMultiplicacao.Match(texto);
                var vlr = 0;
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxDivisao.IsMatch(texto))
            {
                tipo = tipoOperacao.divisao;
                var mtc = rgxDivisao.Match(texto);
                var vlr = 0;
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxSomar.IsMatch(texto))
            {
                tipo = tipoOperacao.somar;
                var mtc = rgxSomar.Match(texto);
                var vlr = 0;
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxSubtrair.IsMatch(texto))
            {
                tipo = tipoOperacao.subtracao;
                var mtc = rgxSubtrair.Match(texto);
                var vlr = 0;
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxAddCaracter.IsMatch(texto))
            {
                tipo = tipoOperacao.add_caracter;
                var vlr = 0;
                if (int.TryParse(texto, out vlr))
                {
                    this.ValorA = vlr;
                }
            }
            else if (rgxSubstituiValor.IsMatch(texto))
            {
                tipo = tipoOperacao.substituir_valor;
                var vlr = 0;
                var mtc = rgxSubstituiValor.Match(texto);
                if (int.TryParse(mtc.Groups[1].Value, out vlr))
                {
                    this.ValorA = vlr;
                }
                if (int.TryParse(mtc.Groups[2].Value, out vlr))
                {
                    this.ValorB = vlr;
                }
            }
            else if (texto == "+/-")
            {
                tipo = tipoOperacao.mais_menos;
            }
            else if (texto == "<<")
            {
                tipo = tipoOperacao.remove_ult_caracter;
            }
            else if (texto == "rev")
            {
                tipo = tipoOperacao.reverse;
            }
            else if (texto == "sum")
            {
                tipo = tipoOperacao.sum;
            }
            else if (texto == ">")
            {
                tipo = tipoOperacao.shiftEsquerda;
            }
            else if (texto == "<")
            {
                tipo = tipoOperacao.shiftDireita;
            }
            else
            {
                throw new Exception("Operação inválida!");
            }
        }

        public string textoOperacao { get; set; }
        public tipoOperacao tipo { get; set; }
        public int ValorA { get; set; }
        public int ValorB { get; set; }

        public enum tipoOperacao
        {
            multiplicacao,
            divisao,
            somar,
            subtracao,
            mais_menos,
            remove_ult_caracter,
            add_caracter,
            reverse,
            substituir_valor,
            sum,
            potencia,
            shiftEsquerda,
            shiftDireita
        }

        public float executarOperacao(float entrada)
        {
            float retorno = 0;
            var str = "";
            bool negativo = false;
            char[] charArray;
            int ncar = 0;
            try
            {
                switch (tipo)
                {
                    case tipoOperacao.add_caracter:
                        retorno = int.Parse($"{entrada}{ValorA}");
                        break;
                    case tipoOperacao.divisao:
                        retorno = entrada / ValorA;
                        break;
                    case tipoOperacao.mais_menos:
                        retorno = -entrada;
                        break;
                    case tipoOperacao.multiplicacao:
                        retorno = entrada * ValorA;
                        break;
                    case tipoOperacao.remove_ult_caracter:
                        str = $"{entrada}";
                        retorno = int.Parse(str.Substring(0, str.Length - 1));
                        break;
                    case tipoOperacao.somar:
                        retorno = entrada + ValorA;
                        break;
                    case tipoOperacao.subtracao:
                        retorno = entrada - ValorA;
                        break;
                    case tipoOperacao.potencia:
                        retorno = (float)Math.Pow(entrada, ValorA);
                        break;
                    case tipoOperacao.substituir_valor:
                        str = $"{entrada}";
                        str = str.Replace($"{ValorA}", $"{ValorB}");
                        retorno = int.Parse(str);
                        break;
                    case tipoOperacao.sum:
                        if (entrada < 0)
                        {
                            negativo = true;
                            entrada = Math.Abs(entrada);
                        }
                        charArray = $"{entrada}".ToCharArray();
                        retorno = 0;
                        foreach (var c in charArray)
                        {
                            var cn = int.Parse(c.ToString());
                            retorno += cn;
                        }

                        if (negativo)
                        {
                            retorno *= -1;
                        }
                        break;
                    case tipoOperacao.reverse:
                        if (entrada < 0)
                        {
                            negativo = true;
                            entrada = Math.Abs(entrada);
                        }
                        charArray = $"{entrada}".ToCharArray();
                        Array.Reverse(charArray);
                        var strRev = new string(charArray);
                        retorno = int.Parse(strRev);
                        if (negativo)
                        {
                            retorno *= -1;
                        }

                        break;
                    case tipoOperacao.shiftEsquerda:
                        if (entrada < 0)
                        {
                            negativo = true;
                            entrada = Math.Abs(entrada);
                        }
                        str = $"{entrada}";
                        if (str.Length > 1)
                        {
                            ncar = (int)Math.Ceiling((double)str.Length / (double)2);
                            var mtcQuebra = Regex.Match(str, $@"(\d+?)(\d{{{ncar}}})");
                            var ndest = mtcQuebra.Groups[1].Value;
                            var nshift = mtcQuebra.Groups[2].Value;
                            str = Regex.Replace(ndest, @"(\d+?)(\d)", $@"$1{nshift}$2");
                            retorno = int.Parse(str);

                            if (negativo)
                            {
                                retorno *= -1;
                            }
                        }

                        break;
                    case tipoOperacao.shiftDireita:
                        if (entrada < 0)
                        {
                            negativo = true;
                            entrada = Math.Abs(entrada);
                        }
                        str = $"{entrada}";
                        if (str.Length > 1)
                        {
                            ncar = (int)Math.Ceiling((double)str.Length / (double)2);
                            var mtcQuebra = Regex.Match(str, $@"(\d{{{ncar}}})(\d+?)");
                            var nshift = mtcQuebra.Groups[1].Value;
                            var ndest = mtcQuebra.Groups[2].Value;
                            str = Regex.Replace(ndest, @"(\d)(\d+?)", $@"$1{nshift}$2");
                            retorno = int.Parse(str);

                            if (negativo)
                            {
                                retorno *= -1;
                            }
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                retorno = entrada;
                throw ex;
            }


            return retorno;

        }
    }
}
