﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;
using Microsoft.FSharp.Core;

namespace NDjango.UnitTests
{
    public partial class Tests
    {
        [Test, TestCaseSource("GetFilters")]
        public void FilterProcess(TestDescriptor test)
        {
            string received = null;
            bool pass = test.Run(out received);
            Assert.IsTrue(pass, String.Format("FAILED - expected \"{0}\" of type \"{1}\", received \"{2}\" of type {3}", test.Result[0],test.Result[0].GetType(), received, received.GetType()));
        }

        public IList<TestDescriptor> GetFilters()
        {
            IList<TestDescriptor> lst = new List<TestDescriptor>();
            lst.Add(new TestDescriptor("filters-escape", "{{ headline|escape }}", ContextObjects.p("headline", "Success<>"), ContextObjects.p("Success&lt;&gt;")));
            lst.Add(new TestDescriptor("add-filter01", "{{ value|add:\"2\" }}", ContextObjects.p("value", 4), ContextObjects.p("6")));
            lst.Add(new TestDescriptor("get_digit-filter01", "{{ value|get_digit:\"1\" }}", ContextObjects.p("value", 123), ContextObjects.p("3")));
            //            >>> get_digit(123, 1)
            //3

            lst.Add(new TestDescriptor("get_digit-filter02", "{{ value|get_digit:\"2\" }}", ContextObjects.p("value", 123), ContextObjects.p("2")));
            //>>> get_digit(123, 2)
            //2

            lst.Add(new TestDescriptor("get_digit-filter02", "{{ value|get_digit:\"2\" }}", ContextObjects.p("value", 123), ContextObjects.p("2")));
            //>>> get_digit(123, 3)
            //1

            lst.Add(new TestDescriptor("get_digit-filter03", "{{ value|get_digit:\"4\" }}", ContextObjects.p("value", 123), ContextObjects.p("0")));
            //>>> get_digit(123, 4)
            //0

            lst.Add(new TestDescriptor("get_digit-filter04", "{{ value|get_digit:\"0\" }}", ContextObjects.p("value", 123), ContextObjects.p("123")));
            //>>> get_digit(123, 0)
            //123

            lst.Add(new TestDescriptor("get_digit-filter05", "{{ value|get_digit:\"0\" }}", ContextObjects.p("value", "xyz"), ContextObjects.p("xyz")));
            //>>> get_digit(u'xyz', 0)
            //u'xyz'


            lst.Add(new TestDescriptor("default-filter01", "{{ value|default:\"default\"}}", ContextObjects.p("value", "val"), ContextObjects.p("val")));
            //>>> default(u"val", u"default")
            //u'val'

            lst.Add(new TestDescriptor("default-filter02", "{{ value|default:\"default\"}}", ContextObjects.p("value", null), ContextObjects.p("default")));
            //>>> default(None, u"default")
            //u'default'

            lst.Add(new TestDescriptor("default-filter03", "{{ value|default:\"default\"}}", ContextObjects.p("value", ""), ContextObjects.p("default")));
            //>>> default(u'', u"default")
            //u'default'

            lst.Add(new TestDescriptor("default-filter04", "{{ value|default:\"0\"}}", ContextObjects.p("value", ""), ContextObjects.p("0")));
            //>>> default(u'', u"default")
            //u'default'

            lst.Add(new TestDescriptor("divisibleby-filter01", "{{value|divisibleby:2}}", ContextObjects.p("value",4),ContextObjects.p("True")));
            //>>> divisibleby(4, 2)
            //True

            lst.Add(new TestDescriptor("divisibleby-filter02", "{{value|divisibleby:3}}", ContextObjects.p("value",4),ContextObjects.p("False")));
            //>>> divisibleby(4, 3)
            //False

            lst.Add(new TestDescriptor("addslashes-filter01", "{% autoescape off %}{{value|addslashes}}{% endautoescape %}", ContextObjects.p("value", "\"double quotes\" and 'single quotes'"), ContextObjects.p("\\\"double quotes\\\" and \\'single quotes\\'")));
            //>>> addslashes(u'"double quotes" and \'single quotes\'')
            //u'\\"double quotes\\" and \\\'single quotes\\\''

            lst.Add(new TestDescriptor("addslashes-filter02", "{{value|addslashes}}", ContextObjects.p("value", "\\ : backslashes, too"), ContextObjects.p("\\\\ : backslashes, too")));
            //>>> addslashes(ur'\ : backslashes, too')
            //u'\\\\ : backslashes, too'

            lst.Add(new TestDescriptor("addslashes-filter03", "{{value|addslashes}}", ContextObjects.p("value", "123"), ContextObjects.p("123")));
            //>>> addslashes(123)
            //u'123'

            lst.Add(new TestDescriptor("capfirst-filter01", "{{value|capfirst}}", ContextObjects.p("value", "hello world"), ContextObjects.p("Hello world")));
            //>>> capfirst(u'hello world')
            //u'Hello world'


            lst.Add(new TestDescriptor("escapejs-filter01", "{% autoescape off %}{{value|escapejs}}{% endautoescape %}", ContextObjects.p("value", "\"double quotes\" and 'single quotes'"), ContextObjects.p("\\x22double quotes\\x22 and \\x27single quotes\\x27")));
            //>>> escapejs(u'"double quotes" and \'single quotes\'')
            //u'\\x22double quotes\\x22 and \\x27single quotes\\x27'

            lst.Add(new TestDescriptor("escapejs-filter02", "{% autoescape off %}{{value|escapejs}}{% endautoescape %}", ContextObjects.p("value", "\\ : backslashes, too"), ContextObjects.p("\\x5C : backslashes, too")));
            //>>> escapejs(ur'\ : backslashes, too')
            //u'\\x5C : backslashes, too'

            lst.Add(new TestDescriptor("escapejs-filter03", "{% autoescape off %}{{value|escapejs}}{% endautoescape %}", ContextObjects.p("value", "and lots of whitespace: \r\n\t\v\f\b"), ContextObjects.p("and lots of whitespace: \\x0D\\x0A\\x09\\x0B\\x0C\\x08")));
            //>>> escapejs(u'and lots of whitespace: \r\n\t\v\f\b')
            //u'and lots of whitespace: \\x0D\\x0A\\x09\\x0B\\x0C\\x08'

            lst.Add(new TestDescriptor("escapejs-filter04", "{% autoescape off %}{{value|escapejs}}{% endautoescape %}", ContextObjects.p("value", "<script>and this</script>"), ContextObjects.p("\\x3Cscript\\x3Eand this\\x3C/script\\x3E")));
            //>>> escapejs(ur'<script>and this</script>')
            //u'\\x3Cscript\\x3Eand this\\x3C/script\\x3E'

            lst.Add(new TestDescriptor("fix_ampersands-filter01", "{% autoescape off %}{{value|fix_ampersands}}{% endautoescape %}", ContextObjects.p("value", "Jack & Jill & Jeroboam"), ContextObjects.p("Jack &amp; Jill &amp; Jeroboam")));
            //>>> fix_ampersands(u'Jack & Jill & Jeroboam')
            //u'Jack &amp; Jill &amp; Jeroboam'

            lst.Add(new TestDescriptor("floatformat-filter01", "{{value|floatformat}}", ContextObjects.p("value", "7.7"), ContextObjects.p("7.7")));
            //>>> floatformat(7.7)
            //u'7.7'

            lst.Add(new TestDescriptor("floatformat-filter02", "{{value|floatformat}}", ContextObjects.p("value", "7.0"), ContextObjects.p("7")));
            //>>> floatformat(7.0)
            //u'7'

            lst.Add(new TestDescriptor("floatformat-filter03", "{{value|floatformat}}", ContextObjects.p("value", "0.7"), ContextObjects.p("0.7")));
            //>>> floatformat(0.7)
            //u'0.7'

            lst.Add(new TestDescriptor("floatformat-filter04", "{{value|floatformat}}", ContextObjects.p("value", "0.07"), ContextObjects.p("0.1")));
            //>>> floatformat(0.07)
            //u'0.1'

            lst.Add(new TestDescriptor("floatformat-filter05", "{{value|floatformat}}", ContextObjects.p("value", "0.007"), ContextObjects.p("0.0")));
            //>>> floatformat(0.007)
            //u'0.0'

            lst.Add(new TestDescriptor("floatformat-filter06", "{{value|floatformat}}", ContextObjects.p("value", "0.0"), ContextObjects.p("0")));
            //>>> floatformat(0.0)
            //u'0'

            lst.Add(new TestDescriptor("floatformat-filter07", "{{value|floatformat:3}}", ContextObjects.p("value", "7.7"), ContextObjects.p("7.700")));
            //>>> floatformat(7.7, 3)
            //u'7.700'

            lst.Add(new TestDescriptor("floatformat-filter08", "{{value|floatformat:3}}", ContextObjects.p("value", "6.000000"), ContextObjects.p("6.000")));
            //>>> floatformat(6.000000, 3)
            //u'6.000'

            lst.Add(new TestDescriptor("floatformat-filter09", "{{value|floatformat:3}}", ContextObjects.p("value", "6.200000"), ContextObjects.p("6.200")));
            //>>> floatformat(6.200000, 3)
            //u'6.200'

            lst.Add(new TestDescriptor("floatformat-filter10", "{{value|floatformat:\"-3\"}}", ContextObjects.p("value", "6.200000"), ContextObjects.p("6.200")));
            //>>> floatformat(6.200000, -3)
            //u'6.200'

            lst.Add(new TestDescriptor("floatformat-filter11", "{{value|floatformat:\"-3\"}}", ContextObjects.p("value", "13.1031"), ContextObjects.p("13.103")));
            //>>> floatformat(13.1031, -3)
            //u'13.103'

            lst.Add(new TestDescriptor("floatformat-filter12", "{{value|floatformat:\"-2\"}}", ContextObjects.p("value", "11.1197"), ContextObjects.p("11.12")));
            //>>> floatformat(11.1197, -2)
            //u'11.12'

            lst.Add(new TestDescriptor("floatformat-filter13", "{{value|floatformat:\"-2\"}}", ContextObjects.p("value", "11.0000"), ContextObjects.p("11")));
            //>>> floatformat(11.0000, -2)
            //u'11'

            lst.Add(new TestDescriptor("floatformat-filter13minus", "{{value|floatformat:\"-2\"}}", ContextObjects.p("value", "11.1000"), ContextObjects.p("11.10")));
            //>>> floatformat(11.0000, -2)
            //u'11'

            lst.Add(new TestDescriptor("floatformat-filter14", "{{value|floatformat:\"-2\"}}", ContextObjects.p("value", "11.000001"), ContextObjects.p("11.00")));
            //>>> floatformat(11.000001, -2)
            //u'11.00'

            lst.Add(new TestDescriptor("floatformat-filter14BIS", "{{value|floatformat:-2}}", ContextObjects.p("value", "11.000001"), ContextObjects.p("11.00")));
            //>>> floatformat(11.000001, -2)
            //u'11.00'

            lst.Add(new TestDescriptor("floatformat-filter13plus", "{{value|floatformat:2}}", ContextObjects.p("value", "11.0000"), ContextObjects.p("11.00")));
            //>>> floatformat(11.0000, -2)
            //u'11'

            lst.Add(new TestDescriptor("floatformat-filter14plus", "{{value|floatformat:2}}", ContextObjects.p("value", "11.000001"), ContextObjects.p("11.00")));
            //>>> floatformat(11.000001, -2)
            //u'11.00'

            lst.Add(new TestDescriptor("floatformat-filter15", "{{value|floatformat:3}}", ContextObjects.p("value", "8.2798"), ContextObjects.p("8.280")));
            //>>> floatformat(8.2798, 3)
            //u'8.280'

            lst.Add(new TestDescriptor("floatformat-filter16", "{{value|floatformat}}", ContextObjects.p("value", "foo"), ContextObjects.p("")));
            //>>> floatformat(u'foo')
            //u''

            lst.Add(new TestDescriptor("floatformat-filter17", "{{value|floatformat:\"bar\"}}", ContextObjects.p("value", "13.1031"), ContextObjects.p("13.1031")));
            //>>> floatformat(13.1031, u'bar')
            //u'13.1031'

            lst.Add(new TestDescriptor("floatformat-filter18", "{{value|floatformat:2}}", ContextObjects.p("value", "18.125"), ContextObjects.p("18.13")));
            //>>> floatformat(18.125, 2) 
            //u'18.13' 

            lst.Add(new TestDescriptor("floatformat-filter19", "{{value|floatformat:\"bar\"}}", ContextObjects.p("value", "foo"), ContextObjects.p("")));
            //>>> floatformat(u'foo', u'bar')
            //u''

            lst.Add(new TestDescriptor("floatformat-filter20", "{{value|floatformat}}", ContextObjects.p("value", "¿Cómo esta usted?"), ContextObjects.p("")));
            //>>> floatformat(u'¿Cómo esta usted?')
            //u''

            lst.Add(new TestDescriptor("floatformat-filter21", "{{value|floatformat}}", ContextObjects.p("value", null), ContextObjects.p("")));
            //>>> floatformat(None)
            //u''

            lst.Add(new TestDescriptor("linenumbers-filter01", "{{value|linenumbers}}", ContextObjects.p("value",123), ContextObjects.p("1. 123")));
            //>>> linenumbers(123)
            //u'1. 123'

            lst.Add(new TestDescriptor("linenumbers-filter02", "{{value|linenumbers}}", ContextObjects.p("value", "line 1\r\nline 2"), ContextObjects.p("1. line 1\r\n2. line 2")));
            //>>> linenumbers(u'line 1\nline 2')
            //u'1. line 1\n2. line 2'

            lst.Add(new TestDescriptor("linenumbers-filter03", "{{value|linenumbers}}", ContextObjects.p("value", "x\nx\nx\nx\nx\nx\nx\nx\nx\nx"), ContextObjects.p("01. x\n02. x\n03. x\n04. x\n05. x\n06. x\n07. x\n08. x\n09. x\n10. x")));
            //>>> linenumbers(u'\n'.join([u'x'] * 10))
            //u'01. x\n02. x\n03. x\n04. x\n05. x\n06. x\n07. x\n08. x\n09. x\n10. x'

            lst.Add(new TestDescriptor("lower-filter01", "{{value|lower}}", ContextObjects.p("value", 123), ContextObjects.p("123")));
            //>>> lower(123)
            //u'123'

            lst.Add(new TestDescriptor("lower-filter02", "{{value|lower}}", ContextObjects.p("value", "TEST"), ContextObjects.p("test")));
            //>>> lower('TEST')
            //u'test'

            lst.Add(new TestDescriptor("lower-filter03", "{{value|lower}}", ContextObjects.p("value", "\xcb"), ContextObjects.p("\xeb")));
            //>>> lower(u'\xcb') # uppercase E umlaut
            //u'\xeb'

            lst.Add(new TestDescriptor("upper-filter01", "{{value|upper}}", ContextObjects.p("value", "Mixed case input"), ContextObjects.p("MIXED CASE INPUT")));
            //>>> upper(u'Mixed case input')
            //u'MIXED CASE INPUT'

            lst.Add(new TestDescriptor("upper-filter02", "{{value|upper}}", ContextObjects.p("value", "\xeb"), ContextObjects.p("\xcb")));
            //>>> upper(u'\xeb') # lowercase e umlaut
            //u'\xcb'

            lst.Add(new TestDescriptor("upper-filter03", "{{value|upper}}", ContextObjects.p("value", 123), ContextObjects.p("123")));
            //>>> upper(123)
            //u'123'

            lst.Add(new TestDescriptor("wordcount-filter01", "{{value|wordcount}}", ContextObjects.p("value", ""), ContextObjects.p("0")));
            //>>> wordcount('')
            //0

            lst.Add(new TestDescriptor("wordcount-filter02", "{{value|wordcount}}", ContextObjects.p("value", "oneword"), ContextObjects.p("1")));
            //>>> wordcount(u'oneword')
            //1

            lst.Add(new TestDescriptor("wordcount-filter03", "{{value|wordcount}}", ContextObjects.p("value", "lots of words"), ContextObjects.p("3")));
            //>>> wordcount(u'lots of words')
            //3

            lst.Add(new TestDescriptor("wordcount-filter04", "{{value|wordcount}}", ContextObjects.p("value", 123), ContextObjects.p("1")));
            //>>> wordcount(123)
            //1

            lst.Add(new TestDescriptor("make_list-filter01", "test{% for testVal in value|make_list %}{{ testVal }}{{ testVal }}{{ testVal }}{% endfor %}", ContextObjects.p("value", "abcd"), ContextObjects.p("testaaabbbcccddd")));
            //>>> make_list('abc')
            //[u'a', u'b', u'c']


            lst.Add(new TestDescriptor("make_list-filter02", "test{% for testVal in value|make_list %}{{ testVal }}{{ testVal }}{{ testVal }}{% endfor %}", ContextObjects.p("value", 1234), ContextObjects.p("test111222333444")));
            ////>>> make_list(1234)
            ////[u'1', u'2', u'3', u'4']

            lst.Add(new TestDescriptor("ljust-filter01", "{{value|ljust:10}}", ContextObjects.p("value", "test"), ContextObjects.p("test      ")));
            //>>> ljust(u'test', 10)
            //u'test      '

            lst.Add(new TestDescriptor("ljust-filter02", "{{value|ljust:3}}", ContextObjects.p("value", "test"), ContextObjects.p("test")));
            //>>> ljust(u'test', 3)
            //u'test'

            lst.Add(new TestDescriptor("ljust-filter03", "{{value|ljust:4}}", ContextObjects.p("value", 123), ContextObjects.p("123 ")));
            //>>> ljust('123', 4)
            //u'123 '

            lst.Add(new TestDescriptor("rjust-filter01", "{{value|rjust:10}}", ContextObjects.p("value", "test"), ContextObjects.p("      test")));
            //>>> rjust(u'test', 10)
            //u'      test'

            lst.Add(new TestDescriptor("rjust-filter02", "{{value|rjust:3}}", ContextObjects.p("value", "test"), ContextObjects.p("test")));
            //>>> rjust(u'test', 3)
            //u'test'

            lst.Add(new TestDescriptor("rjust-filter03", "{{value|rjust:4}}", ContextObjects.p("value", "123"), ContextObjects.p(" 123")));
            //>>> rjust('123', 4)
            //u' 123'

            lst.Add(new TestDescriptor("center-filter01", "{{value|center:5}}", ContextObjects.p("value", "123"), ContextObjects.p(" 123 ")));
            //>>> center('123', 5)
            //u' 123 '

            lst.Add(new TestDescriptor("center-filter02", "{{value|center:6}}", ContextObjects.p("value", "123"), ContextObjects.p(" 123  ")));
            //>>> center('123', 6)
            //u' 123  '

            lst.Add(new TestDescriptor("center-filter03", "{{value|center:6}}", ContextObjects.p("value", "test"), ContextObjects.p(" test ")));
            //>>> center(u'test', 6)
            //u' test '

            lst.Add(new TestDescriptor("cut-filter01", "{{value|cut:\"2\"}}", ContextObjects.p("value", 123), ContextObjects.p("13")));
            //>>> cut(123, '2')
            //u'13'

            lst.Add(new TestDescriptor("cut-filter02", "{{value|cut:\"a\"}}", ContextObjects.p("value", "a string to be mangled"), ContextObjects.p(" string to be mngled")));
            //>>> cut(u'a string to be mangled', 'a')
            //u' string to be mngled'

            lst.Add(new TestDescriptor("cut-filter03", "{{value|cut:\"ng\"}}", ContextObjects.p("value", "a string to be mangled"), ContextObjects.p("a stri to be maled")));
            //>>> cut(u'a string to be mangled', 'ng')
            //u'a stri to be maled'

            lst.Add(new TestDescriptor("cut-filter04", "{{value|cut:\"strings\"}}", ContextObjects.p("value", "a string to be mangled"), ContextObjects.p("a string to be mangled")));
            //>>> cut(u'a string to be mangled', 'strings')
            //u'a string to be mangled'

            lst.Add(new TestDescriptor("title-filter01", "{% autoescape off %}{{value|title}}{% endautoescape %}", ContextObjects.p("value", "a nice title, isn't it?"), ContextObjects.p("A Nice Title, Isn't It?")));
            //>>> title('a nice title, isn\'t it?')
            //u"A Nice Title, Isn't It?"

            lst.Add(new TestDescriptor("title-filter02", "{{value|title}}", ContextObjects.p("value", "discoth\xe8que"), ContextObjects.p("Discoth\xe8que")));
            //>>> title(u'discoth\xe8que')
            //u'Discoth\xe8que'

            lst.Add(new TestDescriptor("title-filter03", "{{value|title}}", ContextObjects.p("value", 123), ContextObjects.p("123")));
            //>>> title(123)
            //u'123'

            lst.Add(new TestDescriptor("removetags-filter01", "{% autoescape off %}{{value|removetags:\"script img\"}}{% endautoescape %}", ContextObjects.p("value", "some <b>html</b> with <script>alert(\"You smell\")</script> disallowed <img /> tags"), ContextObjects.p("some <b>html</b> with alert(\"You smell\") disallowed  tags")));
            ///>>> removetags(u'some <b>html</b> with <script>alert("You smell")</script> disallowed <img /> tags', 'script img')
            ///u'some <b>html</b> with alert("You smell") disallowed  tags'

            lst.Add(new TestDescriptor("force_escape-filter01", "{% autoescape off %}{{value|force_escape}}{% endautoescape %}", ContextObjects.p("value", "<some html & special characters > here"), ContextObjects.p("&lt;some html &amp; special characters &gt; here")));
            ///>>> force_escape(u'<some html & special characters > here')
            ///u'&lt;some html &amp; special characters &gt; here'

            int[] testArrInt = new int[3] { 0, 1, 2 };

            lst.Add(new TestDescriptor("first-filter01", "{{value|first}}", ContextObjects.p("value", testArrInt), ContextObjects.p("0")));
            ///>>> first([0,1,2])
            ///0

            lst.Add(new TestDescriptor("first-filter02", "{{value|first}}", ContextObjects.p("value", string.Empty), ContextObjects.p(string.Empty)));
            ///>>> first(u'')
            ///u''

            lst.Add(new TestDescriptor("first-filter03", "{{value|first}}", ContextObjects.p("value", "test"), ContextObjects.p("t")));
            ///>>> first(u'test')
            ///u't'

            lst.Add(new TestDescriptor("last-filter01", "{{value|last}}", ContextObjects.p("value", testArrInt), ContextObjects.p("2")));
            ///>>> last([0,1,2])
            ///2

            lst.Add(new TestDescriptor("last-filter02", "{{value|last}}", ContextObjects.p("value", string.Empty), ContextObjects.p(string.Empty)));
            ///>>> last(u'')
            ///u''

            lst.Add(new TestDescriptor("last-filter03", "{{value|last}}", ContextObjects.p("value", "othertest"), ContextObjects.p("t")));
            ///>>> last(u'othertest')
            ///u't'

            lst.Add(new TestDescriptor("length-filter01", "{{value|length}}", ContextObjects.p("value", "1234"), ContextObjects.p("4")));
            lst.Add(new TestDescriptor("length-filter02", "{{value|length}}", ContextObjects.p("value", 12345), ContextObjects.p("5")));
            ///>>> length(u'1234')
            ///4

            lst.Add(new TestDescriptor("length-filter03", "{{value|length}}", ContextObjects.p("value", testArrInt), ContextObjects.p("3")));
            ///>>> length([1,2,3])
            ///3

            String[] testArrStrEmpty = new String[] {};
            String[] testArrOneElem = new String[] {"testElem"};

            lst.Add(new TestDescriptor("length_is-filter01", "{{value|length_is:0}}", ContextObjects.p("value", testArrStrEmpty), ContextObjects.p("True")));
            ///>>> length_is([], 0)
            ///True

            lst.Add(new TestDescriptor("length_is-filter02", "{{value|length_is:1}}", ContextObjects.p("value", testArrStrEmpty), ContextObjects.p("False")));
            ///>>> length_is([], 1)
            ///False

            lst.Add(new TestDescriptor("length_is-filter03", "{{value|length_is:1}}", ContextObjects.p("value", "a"), ContextObjects.p("True")));
            ///>>> length_is('a', 1)
            ///True

            lst.Add(new TestDescriptor("length_is-filter04", "{{value|length_is:10}}", ContextObjects.p("value", "a"), ContextObjects.p("False")));
            ///>>> length_is(u'a', 10)
            ///False

            lst.Add(new TestDescriptor("random-filter01", "{{value|random}}", ContextObjects.p("value", testArrOneElem), ContextObjects.p("testElem")));

            lst.Add(new TestDescriptor("slice-filter01", "{{value|slice:0}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p(string.Empty)));
            ///>>> slice(u'abcdefg', u'0')
            ///u''

            lst.Add(new TestDescriptor("slice-filter02", "{{value|slice:1}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p("a")));
            ///>>> slice(u'abcdefg', u'1')
            ///u'a'

            lst.Add(new TestDescriptor("slice-filter03", "{{value|slice:-1}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p("abcdef")));
            ///>>> slice(u'abcdefg', u'-1')
            ///u'abcdef'

            lst.Add(new TestDescriptor("slice-filter04", "{{value|slice:\"1:2\"}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p("b")));
            ///>>> slice(u'abcdefg', u'1:2')
            ///u'b'

            lst.Add(new TestDescriptor("slice-filter05", "{{value|slice:\"1:3\"}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p("bc")));
            ///>>> slice(u'abcdefg', u'1:3')
            ///u'bc'

            lst.Add(new TestDescriptor("slice-filter06", "{{value|slice:\"0::2\"}}", ContextObjects.p("value", "abcdefg"), ContextObjects.p("aceg")));
            ///>>> slice(u'abcdefg', u'0::2')
            ///u'aceg'

            lst.Add(new TestDescriptor("slice-filter07", "test{% for testVal in value|make_list|slice:\"0::2\" %}{{ testVal }}{{ testVal }}{{ testVal }}{% endfor %}", ContextObjects.p("value", "abcd"), ContextObjects.p("testaaaccc")));



            lst.Add(new TestDescriptor("slugify-filter01", "{{value|slugify}}", ContextObjects.p("value", " Jack & Jill like numbers 1,2,3 and 4 and silly characters ?%.$!/"), ContextObjects.p("jack-jill-like-numbers-123-and-4-and-silly-characters")));
            ///>>> slugify(' Jack & Jill like numbers 1,2,3 and 4 and silly characters ?%.$!/')
            /// u'jack-jill-like-numbers-123-and-4-and-silly-characters'
            ///

            lst.Add(new TestDescriptor("slugify-filter03", "{{value|slugify}}", ContextObjects.p("value", "Un \x00e9l\x00e9phant \x00e0 l'or\x00e9e du bois"), ContextObjects.p("un-lphant-lore-du-bois")));
            /// >>> slugify(u"Un \x00e9l\x00e9phant \x00e0 l'or\x00e9e du bois")
            /// u'un-lphant-lore-du-bois'

            lst.Add(new TestDescriptor("truncatewords-filter01", "{{value|truncatewords:1}}", ContextObjects.p("value", "A sentence with a few words in it"), ContextObjects.p("A ...")));
            ///>>> truncatewords(u'A sentence with a few words in it', 1)
            ///u'A ...'

            lst.Add(new TestDescriptor("truncatewords-filter02", "{{value|truncatewords:5}}", ContextObjects.p("value", "A sentence with a few words in it"), ContextObjects.p("A sentence with a few ...")));
            ///>>> truncatewords(u'A sentence with a few words in it', 5)
            ///u'A sentence with a few ...'

            lst.Add(new TestDescriptor("truncatewords-filter02-1", "{{value|truncatewords:8}}", ContextObjects.p("value", "A sentence with a few words in it"), ContextObjects.p("A sentence with a few words in it")));
            lst.Add(new TestDescriptor("truncatewords-filter02-2", "{{value|truncatewords:8}}", ContextObjects.p("value", "A sentence with a few words in it."), ContextObjects.p("A sentence with a few words in it.")));


            lst.Add(new TestDescriptor("truncatewords-filter03", "{{value|truncatewords:100}}", ContextObjects.p("value", "A sentence with a few words in it"), ContextObjects.p("A sentence with a few words in it")));
            ///>>> truncatewords(u'A sentence with a few words in it', 100)
            ///u'A sentence with a few words in it'

            lst.Add(new TestDescriptor("truncatewords-filter04", "{{value|truncatewords:\"not a member\"}}", ContextObjects.p("value", "A sentence with a few words in it"), ContextObjects.p("A sentence with a few words in it")));
            ///>>> truncatewords(u'A sentence with a few words in it', 'not a number')
            ///u'A sentence with a few words in it'

            lst.Add(new TestDescriptor("urlencode-filter01", "{{value|urlencode}}", ContextObjects.p("value", "fran\xe7ois & jill"), ContextObjects.p("fran%c3%a7ois+%26+jill")));
            ///>>> urlencode(u'fran\xe7ois & jill')
            ///u'fran%C3%A7ois%20%26%20jill'

            lst.Add(new TestDescriptor("urlencode-filter02", "{{value|urlencode}}", ContextObjects.p("value", "1"), ContextObjects.p("1")));
            ///>>> urlencode(1)
            ///u'1'

            lst.Add(new TestDescriptor("urlize-filter01", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "http://google.com"), ContextObjects.p("<a href=\"http://google.com\">http://google.com</a>")));
            ////# Check normal urlize
            ////>>> urlize('http://google.com') 
            ////u'<a href="http://google.com" rel="nofollow">http://google.com</a>'

            lst.Add(new TestDescriptor("urlize-filter02", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "http://www.google.com"), ContextObjects.p("<a href=\"http://www.google.com\">http://www.google.com</a>")));
            ////>>> urlize('http://www.google.com/') 
            ////u'<a href="http://www.google.com/" rel="nofollow">http://www.google.com/</a>'

            lst.Add(new TestDescriptor("urlize-filter03", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "www.google.com"), ContextObjects.p("<a href=\"http://www.google.com\">www.google.com</a>")));
            ////>>> urlize('www.google.com') 
            ////u'<a href="http://www.google.com" rel="nofollow">www.google.com</a>'

            lst.Add(new TestDescriptor("urlize-filter04", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "djangoproject.org"), ContextObjects.p("<a href=\"http://djangoproject.org\">djangoproject.org</a>")));
            ////>>> urlize('djangoproject.org') 
            ////u'<a href="http://djangoproject.org" rel="nofollow">djangoproject.org</a>'

            lst.Add(new TestDescriptor("urlize-filter05", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "info@djangoproject.org"), ContextObjects.p("<a href=\"mailto:info@djangoproject.org\">info@djangoproject.org</a>")));
            ////>>> urlize('info@djangoproject.org') 
            ////u'<a href="mailto:info@djangoproject.org">info@djangoproject.org</a>'

            lst.Add(new TestDescriptor("urlize-filter06", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "https://google.com"), ContextObjects.p("<a href=\"https://google.com\">https://google.com</a>")));
            ////# Check urlize with https addresses
            ////>>> urlize('https://google.com') 
            ////u'<a href="https://google.com" rel="nofollow">https://google.com</a>'

            lst.Add(new TestDescriptor("urlize-filter07", "{% autoescape off %}{{value|urlize}}{% endautoescape %}", ContextObjects.p("value", "123"), ContextObjects.p("123")));
            ////>>> urlize(123)
            ////u'123'


            lst.Add(new TestDescriptor("urlizetrunc-fitler01", "{% autoescape off %}{{value|urlizetrunc:20}}{% endautoescape %}",
                                       ContextObjects.p("value", "http://short.com/"),
                                       ContextObjects.p("<a href=\"http://short.com/\">http://short.com/</a>")));
            ////>>> urlizetrunc(u'http://short.com/', 20)
            ////u'<a href="http://short.com/" rel="nofollow">http://short.com/</a>'

            lst.Add(new TestDescriptor("urlizetrunc-fitler02", "{% autoescape off %}{{value|urlizetrunc:20}}{% endautoescape %}",
                                       ContextObjects.p("value", "http://www.google.co.uk/search?hl=en&q=some+long+url&btnG=Search&meta="),
                                       ContextObjects.p("<a href=\"http://www.google.co.uk/search?hl=en&q=some+long+url&btnG=Search&meta=\">http://www.google...</a>")));
            ////>>> urlizetrunc(u'http://www.google.co.uk/search?hl=en&q=some+long+url&btnG=Search&meta=', 20)
            ////u'<a href="http://www.google.co.uk/search?hl=en&q=some+long+url&btnG=Search&meta=" rel="nofollow">http://www.google...</a>'

            string uri = "http://31characteruri.com/test/";


            lst.Add(new TestDescriptor("urlizetrunc-fitler03", "{% autoescape off %}{{value|urlizetrunc:31}}{% endautoescape %}",
                           ContextObjects.p("value", uri),
                           ContextObjects.p("<a href=\"http://31characteruri.com/test/\">http://31characteruri.com/test/</a>")));
            ////>>> urlizetrunc(uri, 31)
            ////u'<a href="http://31characteruri.com/test/" rel="nofollow">http://31characteruri.com/test/</a>'

            lst.Add(new TestDescriptor("urlizetrunc-fitler04", "{% autoescape off %}{{value|urlizetrunc:30}}{% endautoescape %}",
                           ContextObjects.p("value", uri),
                           ContextObjects.p("<a href=\"http://31characteruri.com/test/\">http://31characteruri.com/t...</a>")));

            ////>>> urlizetrunc(uri, 30)
            ////u'<a href="http://31characteruri.com/test/" rel="nofollow">http://31characteruri.com/t...</a>'
            
            lst.Add(new TestDescriptor("urlizetrunc-fitler05", "{% autoescape off %}{{value|urlizetrunc:2}}{% endautoescape %}",
                           ContextObjects.p("value", uri),
                           ContextObjects.p("<a href=\"http://31characteruri.com/test/\">...</a>")));
            ////>>> urlizetrunc(uri, 2)
            ////u'<a href="http://31characteruri.com/test/" rel="nofollow">...</a>'

            lst.Add(new TestDescriptor("wordwrap-filter01", "{% autoescape off %}{{value|wordwrap:14}}{% endautoescape %}",
                                       ContextObjects.p("value",
                                                        "this is a long paragraph of text that really needs to be wrapped I\'m afraid"),
                                       ContextObjects.p(
                                           "this is a long\r\nparagraph of\r\ntext that\r\nreally needs\r\nto be wrapped\r\nI'm afraid")));
            ////>>> wordwrap(u'this is a long paragraph of text that really needs to be wrapped I\'m afraid', 14)
            ////u"this is a long\nparagraph of\ntext that\nreally needs\nto be wrapped\nI'm afraid"

            lst.Add(new TestDescriptor("wordwrap-filter02", "{% autoescape off %}{{value|wordwrap:14}}{% endautoescape %}",
                                       ContextObjects.p("value",
                                                        "this is a short paragraph of text.\r\n  But this line should be indented"),
                                       ContextObjects.p(
                                           "this is a\r\nshort\r\nparagraph of\r\ntext.\r\n  But this\r\nline should be\r\nindented")));
            ////>>> wordwrap(u'this is a short paragraph of text.\n  But this line should be indented',14)
            ////u'this is a\nshort\nparagraph of\ntext.\n  But this\nline should be\nindented'

            lst.Add(new TestDescriptor("wordwrap-filter03", "{% autoescape off %}{{value|wordwrap:15}}{% endautoescape %}",
                                       ContextObjects.p("value",
                                                        "this is a short paragraph of text.\r\n  But this line should be indented"),
                                       ContextObjects.p(
                                           "this is a short\r\nparagraph of\r\ntext.\r\n  But this line\r\nshould be\r\nindented")));
            ////>>> wordwrap(u'this is a short paragraph of text.\n  But this line should be indented',15)
            ////u'this is a short\nparagraph of\ntext.\n  But this line\nshould be\nindented'

            lst.Add(new TestDescriptor("linebreaks-filter01", "{% autoescape off %}{{value|linebreaks}}{% endautoescape %}",
                                        ContextObjects.p("value", "line 1"),
                                        ContextObjects.p("<p>line 1</p>")));
            ////>>> linebreaks(u'line 1')
            ////u'<p>line 1</p>'

            lst.Add(new TestDescriptor("linebreaks-filter02", "{% autoescape off %}{{value|linebreaks}}{% endautoescape %}",
                                        ContextObjects.p("value", "line 1\nline 2"),
                                        ContextObjects.p("<p>line 1<br />line 2</p>")));
            ////>>> linebreaks(u'line 1\nline 2')
            ////u'<p>line 1<br />line 2</p>'

            lst.Add(new TestDescriptor("linebreaks-filter02-bis", "{% autoescape off %}{{value|linebreaks}}{% endautoescape %}",
                                        ContextObjects.p("value", "line 1\n\nline 2"),
                                        ContextObjects.p("<p>line 1</p><p>line 2</p>")));

            lst.Add(new TestDescriptor("linebreaks-filter03", "{% autoescape off %}{{value|linebreaks}}{% endautoescape %}",
                                        ContextObjects.p("value", 123),
                                        ContextObjects.p("<p>123</p>")));
            ////>>> linebreaks(123)
            ////u'<p>123</p>'

            lst.Add(new TestDescriptor("linebreaksbr-filter01", "{% autoescape off %}{{value|linebreaksbr}}{% endautoescape %}",
                                        ContextObjects.p("value", "line 1\n\nline 2"),
                                        ContextObjects.p("line 1<br /><br />line 2")));


            lst.Add(new TestDescriptor("striptags-filter01", "{% autoescape off %}{{value|striptags}}{% endautoescape %}",
                                        ContextObjects.p("value", "some <b>html</b> with <script>alert(\"Test alert\")</script> disallowed <img /> tags"), ContextObjects.p("some html with alert(\"Test alert\") disallowed  tags")));
            ////>>> join([0,1,2], u'glue')
            ////u'0glue1glue2'

            lst.Add(new TestDescriptor("join-filter01", "{{value|join:\"glue\"}}",
                                        ContextObjects.p("value", ContextObjects.range(3)),
                                        ContextObjects.p("0glue1glue2")));


            lst.Add(new TestDescriptor("join-filter02", "{{value|join:\"glue\"}}",
                            ContextObjects.p("value", 345),
                            ContextObjects.p(typeof(System.Exception))));


            lst.Add(new TestDescriptor("yesno-filter01", "{{value|yesno}}",
                                        ContextObjects.p("value",true),
                                        ContextObjects.p("yes")));
            ////>>> yesno(True)
            ////u'yes'

            lst.Add(new TestDescriptor("yesno-filter02", "{{value|yesno}}",
                                        ContextObjects.p("value", false),
                                        ContextObjects.p("no")));
            ////>>> yesno(False)
            ////u'no'


            lst.Add(new TestDescriptor("yesno-filter03", "{{value|yesno:\"certainly,get out of town,perhaps\"}}",
                                        ContextObjects.p("value", true),
                                        ContextObjects.p("certainly")));

            lst.Add(new TestDescriptor("yesno-filter04", "{{value|yesno:\"certainly,get out of town,perhaps\"}}",
                            ContextObjects.p("value", false),
                            ContextObjects.p("get out of town")));
            ////>>> yesno(False, u'certainly,get out of town,perhaps')
            ////u'get out of town'

            lst.Add(new TestDescriptor("yesno-filter05", "{{value|yesno:\"certainly,get out of town\"}}",
                ContextObjects.p("value", false),
                ContextObjects.p("get out of town")));


            var testList = new List<Dictionary<string, IComparable>>();
            var dict1 = ContextObjects.dictComp("age", 23, "name", "Barbara-Ann");
            var dict2 = ContextObjects.dictComp("age", 63, "name", "Ra Ra Rasputin");
            var dict3 = ContextObjects.dictComp("name", "Jonny B Goode", "age", 18);
            testList.Add(dict1);
            testList.Add(dict2);
            testList.Add(dict3);

            var testList1 = new List<Dictionary<string, IComparable>>();
            var dictt1 = ContextObjects.dictComp("age", "cc", "name", "Barbara-Ann");
            var dictt2 = ContextObjects.dictComp("age", "bbb", "name", "Ra Ra Rasputin");
            var dictt3 = ContextObjects.dictComp("name", "Jonny B Goode", "age", "aa");
            testList1.Add(dictt1);
            testList1.Add(dictt2);
            testList1.Add(dictt3);


            lst.Add(new TestDescriptor("dictsort-filter01", "test1-{% for testVal in value|dictsort:\"age\" %}age:{{testVal.age}};name:{{testVal.name}};;{% endfor %}",
                ContextObjects.p("value", testList),
                ContextObjects.p("test1-age:18;name:Jonny B Goode;;age:23;name:Barbara-Ann;;age:63;name:Ra Ra Rasputin;;")));

            lst.Add(new TestDescriptor("dictsortreversed-filter01", "test2-{% for testVal in value|dictsortreversed:\"age\" %}age:{{testVal.age}};name:{{testVal.name}};;{% endfor %}",
                ContextObjects.p("value", testList),
                ContextObjects.p("test2-age:63;name:Ra Ra Rasputin;;age:23;name:Barbara-Ann;;age:18;name:Jonny B Goode;;")));

            lst.Add(new TestDescriptor("dictsort-filter02", "test3-{% for testVal in value|dictsort:\"age\" %}age:{{testVal.age}};name:{{testVal.name}};;{% endfor %}",
                ContextObjects.p("value", testList1),
                ContextObjects.p("test3-age:aa;name:Jonny B Goode;;age:bbb;name:Ra Ra Rasputin;;age:cc;name:Barbara-Ann;;")));

            lst.Add(new TestDescriptor("dictsortreversed-filter02", "test4-{% for testVal in value|dictsortreversed:\"age\" %}age:{{testVal.age}};name:{{testVal.name}};;{% endfor %}",
                ContextObjects.p("value", testList1),
                ContextObjects.p("test4-age:cc;name:Barbara-Ann;;age:bbb;name:Ra Ra Rasputin;;age:aa;name:Jonny B Goode;;")));

            return lst;
        }





    }
}
