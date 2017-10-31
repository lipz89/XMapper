# XMapper
一个模型转换类库

XMapper	是我自己原来写的，通过拼装C#代码实现的。

后来发现网上有个更好的开源类库[TinyMapper](https://github.com/TinyMapper/TinyMapper "TinyMapper"),通过Emit技术生成转换代码，但是其中有一些瑕疵，我自己又做了一些调整，现在将调整后的代码放在这里。调整是基于TimyMapper2.0.8版本。

###调整内容

>###1. 调整侧重点
	原来的TinyMapper是以源对象为侧重点，将源对象的属性映射到目标对象的属性上。
	调整后是以目标对象为侧重点，通过对目标对象属性的映射规则从源对象取值，可以通过一个Func表达式对源对象处理，将返回结果映射到目标类型的属性。

>###2. 调整映射规则的使用范围
	原来的TinyMapper通过Bind方法定义源类型到目标类型的映射规则，使用的时候先按该映射规则生成映射代码，如果其他类型的属性中包含了已定义映射规则的类型，却不会重复使用该规则，而是自动按默认映射规则再次生成一个映射类的代码，而且如果一个类型被多个类型作为属性，会生成映射代码多次，这与实际需求不符。
	调整以后会重复使用规则，不会生成多余的映射类。

>###3. 调整映射接口方法
	原来的TinyMapper遇到递归属性会报错（无限递归）。
	调整后会自动映射递归属性，其中过程为先将映射过的对象缓存起来，如果遇到该对象再次映射则直接取缓存，递归属性不会无限递归。

>###4. 丰富了集合映射类型
	原来的TinyMapper能映射的集合类型有限，本版本丰富了集合映射类型。

>###5. 增加了查看类型映射规则的接口
	原来的TinyMapper只是将映射规则MappingMember作为程序集内部使用，本版本提供了查看接口GetMemberBinding，以供外部使用。

>###6. 增加了映射表达式树的相关方法类型
	增加了MapperTransfer<TSource,TTarget>类型，可以将Func<TSource,bool>转换成Func<TTarget,bool>。