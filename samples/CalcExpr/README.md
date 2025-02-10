# Simple Calc

This project implements a simple mathematical expression parser.

## Simple Expression Grammar

```sh
start
  =  sum_expr EOF
  ;

sum_expr
  = mul_expr (S [+-] mul_expr)*
  ;

mul_expr
  = unary_expr (S [*/] unary_expr)*
  ;

unary_expr
  = S "-"? primary_expr
  ;

primary_expr
  = parenthesis_expr / number_expr
  ;

parenthesis_expr
  = S "(" Sum S ")"
  ;

number_expr
  =  S [0-9]+
  ;

S
  = [ \t\r\n]*
  ;

EOF:
  = $
  ;
```
