# Simple Calc

This project implements a simple mathematical expression parser.

## Simple Expression Grammar

```text
start
  = S sum_expr EOF
  ;

sum_expr
  = mul_expr ([+-] S mul_expr)*
  ;

mul_expr
  = unary_expr ([*/] S unary_expr)*
  ;

unary_expr
  = "-"? S primary_expr
  ;

primary_expr
  = parenthesis_expr / number_expr
  ;

parenthesis_expr
  = "(" S sum_expr ")" S
  ;

number_expr
  = number S
  ;

number
  = [+-]? [0-9]+ ("." [0-9]+)? ([eE] [+-]? [0-9]+)?
  ;

S
  = [\s]*
  ;

EOF
  = $
  ;
```
