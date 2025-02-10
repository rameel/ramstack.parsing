# JSON Parser

This project implements a simple JSON parser.

## JSON Grammar

```sh
start
  = value $
  ;

value
  = S (
    object
    / array
    / string
    / number
    / "true"
    / "false"
    / "null"
    )
  ;

object
  = S "{" (member (S "," S member)*)? S "}"
  ;

member
  = string S ":" value
  ;

array
  = S "[" (value (S "," value)*)? S "]"
  ;

S
  = [ \t\r\n]*
  ;
```
