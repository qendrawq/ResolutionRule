using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ResolutionRule {

    class Program {

        public enum Operand {
            Empty,
            And,
            Or,
            Not
        }

        public struct Formula {

            public class Statement : IEquatable<char> {

                public Statement(string literal) {

                    this.IsNegation = literal.Count() > 1;
                    this.Literal = literal.Last();
                }

                public char Literal {
                    get; set;
                }

                public bool IsNegation {
                    get; set;
                }

                public bool Equals(char other) => this.Literal.Equals(other);
            }

            public Formula(Statement s1, Statement s2, Operand operand) {

                this.S1 = s1;
                this.S2 = s2;
                this.Operand = operand;
            }

            public Operand Operand {
                get; set;
            }

            public Statement S1 {
                get; set;
            }

            public Statement S2 {
                get; set;
            }

            public bool Contains(char ch) => S1.Literal.Equals(ch) || S2.Literal.Equals(ch);

            public bool ContainsNegation(char ch) => (S1 != null && S1.Literal.Equals(ch) && S1.IsNegation) || (S2 != null && S2.Literal.Equals(ch) && S2.IsNegation);

            public bool IsEmpty() => S1 == null && S2 == null;

            public static Formula[] ConvertToFormulaList(string[] text) => text.Select(f => ConvertToFormula(f)).ToArray();

            public static Formula ConvertToFormula(string text) {

                var statement = Regex.Matches(text, @"[!]*\w");
                if (!statement.Any())
                    throw new ArgumentException();

                string s1 = statement[0].Value;
                if (statement.Count == 1)
                    return new Formula(new Statement(s1), null, Operand.Empty);

                string s2 = statement[1].Value,
                       operand = text.Replace(s1, string.Empty).Replace(s2, string.Empty);

                return new Formula(new Statement(s1), new Statement(s2), GetOperand(operand));
            }

            public Formula GetResolvent(Formula formula) {

                char? literal1 = this.S1?.Literal,
                     literal2 = this.S2?.Literal;
                if (literal1.HasValue && formula.ContainsNegation(literal1.Value)) {
                    formula.Remove(literal1);
                    this.Remove(literal1);
                }
                else if (literal2.HasValue && formula.ContainsNegation(literal2.Value)) {
                    formula.Remove(literal2);
                    this.Remove(literal2);
                }

                literal1 = formula.S1?.Literal;
                literal2 = formula.S2?.Literal;
                if (literal1.HasValue && this.ContainsNegation(literal1.Value)) {
                    this.Remove(literal1);
                    formula.Remove(literal1);
                }
                else if (literal2.HasValue && this.ContainsNegation(literal2.Value)) {
                    this.Remove(literal2);
                    formula.Remove(literal2);
                }

                return new Formula(this.S1 ?? this.S2, formula.S1 ?? formula.S2, Operand.Or);
            }

            private void Remove(char? literal) {

                if (!literal.HasValue)
                    return;

                if (S1?.Equals(literal.Value) ?? false)
                    S1 = null;
                if (S2?.Equals(literal.Value) ?? false)
                    S2 = null;
            }

            private static Operand GetOperand(string operand) {

                switch (operand.First()) {
                    case '|':
                        return Operand.Or;
                    case '&':
                        return Operand.And;
                    case '!':
                        return Operand.Not;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        static void Main(string[] args) {

            var formulasText = new string[] { "C|P", "!C|R", "!P|H", "!H" };
            var formulas = Formula.ConvertToFormulaList(formulasText);
            Console.WriteLine($"Initial: {string.Join(' ', formulasText)}");

            var searched = new Formula.Statement("R");
            searched.IsNegation = !searched.IsNegation;
            Console.WriteLine($"Check: {searched.Literal}");

            var searchedFormula = new Formula(searched, null, Operand.Empty);
            var resolvent = formulas[0].GetResolvent(formulas[1]);
            for (int i = 2; i < formulas.Count(); i++)
                resolvent = formulas[i].GetResolvent(resolvent);
            resolvent = searchedFormula.GetResolvent(resolvent);
            Console.WriteLine($"Result: {resolvent.IsEmpty()}");

            Console.ReadKey();
        }
    }
}
