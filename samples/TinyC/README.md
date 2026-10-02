# Tiny-C

This project implements a parser for the [Tiny-C](http://www.iro.umontreal.ca/~felipe/IFT2030-Automne2002/Complements/tinyc.c) language,
a highly simplified version of `C` designed as an educational tool for learning about compilers.

The main differences from the original `Tiny-C` are:
- Variable names are not limited to single letters.
- Additional operators are supported.
- Line comments (`// ...`) and block comments (`/* ... */`) are supported.

## Tiny-C Grammar

```
start
  = S statement EOF
  ;

keyword
  = while_keyword / do_keyword / if_keyword / else_keyword
  ;

identifier_part
  = [a-zA-Z_0-9]
  ;

while_keyword
  = "while" !identifier_part
  ;

do_keyword
  = "do" !identifier_part
  ;

if_keyword
  = "if" !identifier_part
  ;

else_keyword
  = "else" !identifier_part
  ;

number
  = [0-9]+
  ;

variable
  = !keyword [a-z] identifier_part*
  ;

S
  = (single_comment / multiline_comment / [\s])*
  ;

single_comment
  = "//" (!EOL .)* EOL
  ;

multiline_comment
  = "/*" (!"*/" .)* "*/"
  ;

EOL
  = "\r\n" / [\r\n\u0085\u2028\u2029] / EOF
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
  = assignment_expr
  / ternary_expr
  ;

assignment_expr
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
  = shift_expr (("<=" / "<" / ">=" / ">") S shift_expr)*
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
  = if_keyword S "(" S expr ")" S statement (else_keyword S statement)?
  ;

while_statement
  = while_keyword S "(" S expr ")" S statement
  ;

do_while_statement
  = do_keyword S statement while_keyword S "(" S expr ")" S ";" S
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
