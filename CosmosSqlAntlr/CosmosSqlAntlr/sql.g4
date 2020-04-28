﻿grammar sql;

program
	: sql_query EOF
	;

sql_query
	: select_clause from_clause? where_clause? group_by_clause? order_by_clause? offset_limit_clause?
	;

select_clause
	: K_SELECT K_DISTINCT? top_spec? selection
	;

top_spec 
	: K_TOP NUMERIC_LITERAL
	;

selection
	: select_star_spec
	| select_value_spec 
	| select_list_spec
	;

select_star_spec
	: '*'
	;

select_value_spec
	: K_VALUE scalar_expression
	;

select_list_spec
	: select_item ( ',' select_item )*
	;

select_item
	: scalar_expression (K_AS alias)?
	;

from_clause
	: '$'
	;

where_clause
	: '%'
	;

group_by_clause
	: '%'
	;

order_by_clause
	: '%'
	;

offset_limit_clause
	: '%'
	;

scalar_expression
	: '[' scalar_expression_list? ']' #ArrayCreateScalarExpression
	| K_ARRAY '(' sql_query ')' #ArrayScalarExpression
	| scalar_expression K_NOT? K_BETWEEN scalar_expression K_AND scalar_expression #BetweenScalarExpression
	| scalar_expression ( '+' | K_AND | '&' | '|' | '^' | '/' | '=' | '>' | '>=' | '<' | '<=' | '%' | '*' | '!=' | K_OR | '||' | '-' ) scalar_expression #BinaryScalarExpression
	| scalar_expression '??' scalar_expression #CoalesceScalarExpression
	| scalar_expression '?' scalar_expression ':' scalar_expression #ConditionalScalarExpression
	| K_EXISTS '(' sql_query ')' #ExistsScalarExpression
	| (K_UDF '.')? IDENTIFIER '(' scalar_expression_list? ')' #FunctionCallScalarExpression
	| scalar_expression K_NOT? K_IN '(' scalar_expression_list? ')' #InScalarExpression
	| literal #LiteralScalarExpression
	| scalar_expression '[' scalar_expression ']' #MemberIndexerScalarExpression
	| '{' object_property_list? '}' #ObjectCreateScalarExpression
	| scalar_expression '.' IDENTIFIER #PropertyRefScalarExpression
	| '(' sql_query ')' #SubqueryScalarExpression
	| ( '-' | '+' | '~' | '!' ) scalar_expression #UnaryScalarExpression
	;

scalar_expression_list 
	: scalar_expression ( ',' scalar_expression )*
	;

K_AND : A N D;
K_ARRAY : A R R A Y;
K_AS : A S;
K_BETWEEN : B E T W E E N;
K_DISTINCT : D I S T I N C T;
K_EXISTS : E X I S T S;
K_FALSE : F A L S E;
K_IN : I N ;
K_NOT : N O T;
K_NULL : N U L L;
K_OR : O R;
K_SELECT : S E L E C T;
K_TOP : T O P;
K_TRUE : T R U E;
K_UDF : U D F;
K_UNDEFINED : U N D E F I N E D;
K_VALUE : V A L U E;

WS
   : [ \r\n\t] + -> skip
   ;

object_property_list
	: object_property (',' object_property)*
	;

object_property
	: STRING_LITERAL ':' scalar_expression
	;

literal
	: STRING_LITERAL
	| NUMERIC_LITERAL
	| K_TRUE
	| K_FALSE
	| K_NULL
	| K_UNDEFINED
	;

alias
	: '%'
	;

NUMERIC_LITERAL
	: ( '+' | '-' )? DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )?
	| ( '+' | '-' )? '.' DIGIT+ ( E [-+]? DIGIT+ )?
	;

STRING_LITERAL
	: ('\'' | '"') ( ~'\'' | '\'\'' )* ('\'' | '"')
	;

IDENTIFIER
	:
	| [a-zA-Z_]+
	;

fragment DIGIT : [0-9];

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];