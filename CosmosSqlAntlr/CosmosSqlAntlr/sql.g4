grammar sql;

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
	: K_TOP signed_number
	;

selection
	: select_list_spec
	| select_value_spec
	| select_star_spec
	;

select_list_spec
	: select_item ( ',' select_item )*
	;

select_value_spec
	: K_VALUE scalar_expression
	;

select_star_spec
	: '*'
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
	: '%'
	;

alias
	: '%'
	;

signed_number
	: ( '+' | '-' )? NUMERIC_LITERAL
	;

NUMERIC_LITERAL
	: DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )?
	| '.' DIGIT+ ( E [-+]? DIGIT+ )?
	;

K_AS : A S;
K_DISTINCT : D I S T I N C T;
K_SELECT : S E L E C T;
K_TOP : T O P;
K_VALUE : V A L U E;

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