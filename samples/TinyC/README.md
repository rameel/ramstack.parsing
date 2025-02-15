# Tiny-C

This project implements a parser for the [Tiny-C](http://www.iro.umontreal.ca/~felipe/IFT2030-Automne2002/Complements/tinyc.c) language, a highly simplified version of `C` designed as an educational tool for learning about compilers.

All variables are predefined, of integer type, and initialized to zero.

The main differences from the original `Tiny-C` are:
- Variable names are not limited to single letters.
- Additional operators are supported.

## Tiny-C Grammar

```sh
start
  = S statement EOF
  ;

keyword
  = ("while" / "do" / "if" / "else") ![\w]
  ;

number
  = [0-9]+
  ;

variable
  = !keyword [a-zA-Z_][a-zA-Z0-9_]*
  ;

S
  = [ \t\n\r]*
  ;

EOF
  = $
  ;

var_expr
  = variable S
  ;

number_expr
  = number S
  ;

expr
  = assigment_expr
  / ternary_expr
  ;

assigment_expr
  = var_expr "=" S expr
  ;

ternary_expr
  = logical_or_expr ("?" S expr ":" S ternary_expr)?
  ;

logical_or_expr
  = logical_and_expr ("||" S logical_and_expr)*
  ;

logical_and_expr
  = bitwise_or_expr ("&&" S bitwise_or_expr)*
  ;

bitwise_or_expr
  = bitwise_xor_expr ("|" S bitwise_xor_expr)*
  ;

bitwise_xor_expr
  = bitwise_and_expr ("^" S bitwise_and_expr)*
  ;

bitwise_and_expr
  = eq_expr ("&" S eq_expr)*
  ;

eq_expr
  = relational_expr (("==" / "!=") S relational_expr)*
  ;

relational_expr
  = shift_expr (("<" / "<=" / ">" / ">=") S shift_expr)*
  ;

shift_expr
  = sum_expr (("<<" / ">>") S sum_expr)*
  ;

sum_expr
  = mul_expr ([+-] S mul_expr)*
  ;

mul_expr
  = unary_expr ([*/%] S unary_expr)*
  ;

unary_expr
  = [-+~!]? S primary_expr
  ;

primary_expr
  = parenthesis
  / var_expr
  / number_expr
  ;

parenthesis
  = "(" S expr ")" S
  ;

statement
  = if_statement
  / while_statement
  / do_while_statement
  / block_statement
  / expr_statement
  / empty_statement
  ;

if_statement
  = "if" S "(" S expr ")" S statement ("else" S statement)?
  ;

while_statement
  = "while" S "(" S expr ")" S statement
  ;

do_while_statement
  = "do" S statement "while" S "(" S expr ")" S ";" S
  ;

block_statement
  = "{" S statement* "}" S
  ;

expr_statement
  = expr ";" S
  ;

empty_statement
  = ";" S
  ;
```
