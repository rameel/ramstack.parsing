# JSON Parser

This project implements a simple JSON parser.

```text
start
  = S value $
  ;

value
  = (object / array / string / number / "true" / "false" / "null") S
  ;

object
  = "{" S (member ("," S member)*)? "}" S
  ;

member
  = string S ":" S value
  ;

array
  = "[" S (value ("," S value)*)? "]" S
  ;

S
  = [\s]*
  ;
```
